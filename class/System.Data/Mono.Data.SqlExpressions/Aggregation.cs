//
// Aggregation.cs
//
// Author:
//   Juraj Skripsky (juraj@hotfeet.ch)
//
// (C) 2004 HotFeet GmbH (http://www.hotfeet.ch)
//

using System;
using System.Collections;
using System.Data;

namespace Mono.Data.SqlExpressions {
	internal enum AggregationFunction {
		Count, Sum, Min, Max, Avg, StDev, Var
	}

	internal class Aggregation : IExpression {
		bool cacheResults;
		DataRow[] rows;
		ColumnReference column;
		AggregationFunction function;
		int count;
		IConvertible result;
	
		public Aggregation (bool cacheResults, DataRow[] rows, AggregationFunction function, ColumnReference column)
		{
			this.cacheResults = cacheResults;
			this.rows = rows;
			this.column = column;
			this.function = function;
			this.result = null;
		}
	
		public object Eval (DataRow row)
		{
			//TODO: implement a better caching strategy and a mechanism for cache invalidation.
			//for now only aggregation over the table owning 'row' (e.g. 'sum(parts)'
			//in constrast to 'sum(parent.parts)' and 'sum(child.parts)') is cached.
			if (cacheResults && result != null && column.ReferencedTable == ReferencedTable.Self)
				return result;
				
			count = 0;
			result = null;
			
			object[] values;
			if (rows == null)
				values = column.GetValues (column.GetReferencedRows (row));
			else
				values = column.GetValues (rows);
			
			foreach (object val in values) {
				if (val == null)
					continue;
					
				count++;
				Aggregate ((IConvertible)val);
			}
			
			switch (function) {
			case AggregationFunction.StDev:
			case AggregationFunction.Var:
				result = CalcStatisticalFunction (values);
				break;
					
			case AggregationFunction.Avg:
				result = Numeric.Divide (result, count);
				break;
			
			case AggregationFunction.Count:
				result = count;
				break;
			}
			
			if (result == null)
				result = 0;
				
			return result;
		}
		
		private void Aggregate (IConvertible val)
		{
			switch (function) {
			case AggregationFunction.Min:
				result = (result != null ? Numeric.Min (result, val) : val);
				return;
			
			case AggregationFunction.Max:
				result = (result != null ? Numeric.Max (result, val) : val);
				return;

			case AggregationFunction.Sum:
			case AggregationFunction.Avg:
			case AggregationFunction.StDev:
 			case AggregationFunction.Var:
				result = (result != null ? Numeric.Add (result, val) : val);
				return;
			}
		}
		
		private IConvertible CalcStatisticalFunction (object[] values)
		{
			double average = (double)Convert.ChangeType(result, TypeCode.Double) / count;
			double res = 0.0;
						
			foreach (object val in values) {
				if (val == null)
					continue;
					
				double diff = average - (double)Convert.ChangeType(val, TypeCode.Double);
				res += System.Math.Pow (diff, 2);
			}
			res /= (count - 1);
			
			if (function == AggregationFunction.StDev)
				res = System.Math.Sqrt (res);
			
			return res;
		}
	}
}
