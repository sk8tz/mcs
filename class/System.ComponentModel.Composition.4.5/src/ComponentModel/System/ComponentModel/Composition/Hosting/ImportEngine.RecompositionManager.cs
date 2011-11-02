// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Internal;
using Microsoft.Internal.Collections;

namespace System.ComponentModel.Composition.Hosting
{
    public partial class ImportEngine
    {
        /// <summary>
        ///     Used by the <see cref="ImportEngine"/> to effiecently store and retrieve the list of parts
        ///     that will be affected by changes to exports. This allows the <see cref="ImportEngine"/> to properly
        ///     block breaking changes and also recompose imports as appropriate.
        /// </summary>
        private class RecompositionManager
        {
            private WeakReferenceCollection<PartManager> _partsToIndex = new WeakReferenceCollection<PartManager>();
            private WeakReferenceCollection<PartManager> _partsToUnindex = new WeakReferenceCollection<PartManager>();
            private Dictionary<string, WeakReferenceCollection<PartManager>> _partManagerIndex = new Dictionary<string, WeakReferenceCollection<PartManager>>();

            public void AddPartToIndex(PartManager partManager)
            {
                this._partsToIndex.Add(partManager);
            }

            public void AddPartToUnindex(PartManager partManager)
            {
                this._partsToUnindex.Add(partManager);
            }

            public IEnumerable<PartManager> GetAffectedParts(IEnumerable<string> changedContractNames)
            {
                this.UpdateImportIndex();

                List<PartManager> parts = new List<PartManager>();

                parts.AddRange(GetPartsImporting(ImportDefinition.EmptyContractName));

                foreach (string contractName in changedContractNames)
                {
                    parts.AddRange(GetPartsImporting(contractName));
                }

                return parts;
            }

            public static IEnumerable<ImportDefinition> GetAffectedImports(ComposablePart part, IEnumerable<ExportDefinition> changedExports)
            {
                return part.ImportDefinitions.Where(import => IsAffectedImport(import, changedExports));
            }

            private static bool IsAffectedImport(ImportDefinition import, IEnumerable<ExportDefinition> changedExports)
            {
                // This could be more efficient still if the export definitions were indexed by contract name,
                // only worth revisiting if we need to squeeze more performance out of recomposition
                foreach (var export in changedExports)
                {
                    if (import.IsConstraintSatisfiedBy(export))
                    {
                        return true;
                    }
                }
               
                return false;
            }

            public IEnumerable<PartManager> GetPartsImporting(string contractName)
            {
                WeakReferenceCollection<PartManager> partManagerList;
                if (!this._partManagerIndex.TryGetValue(contractName, out partManagerList))
                {
                    return Enumerable.Empty<PartManager>();
                }

                return partManagerList.AliveItemsToList();
            }

            private void AddIndexEntries(PartManager partManager)
            {
                foreach (string contractName in partManager.GetImportedContractNames())
                {
                    WeakReferenceCollection<PartManager> indexEntries;
                    if (!this._partManagerIndex.TryGetValue(contractName, out indexEntries))
                    {
                        indexEntries = new WeakReferenceCollection<PartManager>();
                        this._partManagerIndex.Add(contractName, indexEntries);
                    }

                    if (!indexEntries.Contains(partManager))
                    {
                        indexEntries.Add(partManager);
                    }
                }
            }

            private void RemoveIndexEntries(PartManager partManager)
            {
                foreach (string contractName in partManager.GetImportedContractNames())
                {
                    WeakReferenceCollection<PartManager> indexEntries;
                    if (this._partManagerIndex.TryGetValue(contractName, out indexEntries))
                    {
                        indexEntries.Remove(partManager);
                        var aliveItems = indexEntries.AliveItemsToList();

                        if (aliveItems.Count == 0)
                        {
                            this._partManagerIndex.Remove(contractName);
                        }
                    }
                }
            }

            private void UpdateImportIndex()
            {
                var partsToIndex = this._partsToIndex.AliveItemsToList();
                this._partsToIndex.Clear();

                var partsToUnindex = this._partsToUnindex.AliveItemsToList();
                this._partsToUnindex.Clear();

                if (partsToIndex.Count == 0 && partsToUnindex.Count == 0)
                {
                    return;
                }

                foreach (var partManager in partsToIndex)
                {
                    var index = partsToUnindex.IndexOf(partManager);

                    // If the same part is being added and removed we can ignore both
                    if (index >= 0)
                    {
                        partsToUnindex[index] = null;
                    }
                    else
                    {
                        AddIndexEntries(partManager);
                    }
                }

                foreach (var partManager in partsToUnindex)
                {
                    if (partManager != null)
                    {
                        RemoveIndexEntries(partManager);
                    }
                }
            }
        }
    }
}
