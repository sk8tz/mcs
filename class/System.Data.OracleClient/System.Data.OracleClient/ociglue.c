// 
// ociglue.c -  provides glue between 
//              managed C#/.NET System.Data.OracleClient.dll and 
//              unmanaged native c library oci.dll
//              to be used in Mono System.Data.OracleClient as
//              the Oracle 8i data provider.
//  
// Part of unmanaged C library System.Data.OracleClient.ociglue.dll
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient.OCI
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient
// 
// Author: 
//     Daniel Morgan <danmorg@sc.rr.com>
//         
// Copyright (C) Daniel Morgan, 2002
// 
// Licensed under the MIT/X11 License.
//

#include "ociglue.h"

/* ------------- Global Variables ---------------------- */
GSList *conlist = NULL; /* singly linked list of oci_glue_connection_t */

/* ------------- Private Function Prototypes ----------- */
oci_glue_connection_t *find_connection (ub4 connection_handle);
GSList *find_connection_node (ub4 connection_handle);
text *check_error_internal (oci_glue_connection_t *oci_glue_handle, sb4 *errcode);

/* ------------- Public Functions ----------- */
text *OciGlue_Connect (sword *status, ub4 *connection_handle, sb4 *errcode, 
					   char *database, char *username, char *password)
{
	oci_glue_connection_t *oci_glue_handle;
	text *errbuf = NULL;

	if(!connection_handle || !errcode || !database || !username || !password) {
		return NULL;
	}
	
	*errcode = 0;
	
	*connection_handle = 0;

	oci_glue_handle = g_new(oci_glue_connection_t, 1);

	*connection_handle = (ub4) oci_glue_handle;
	
	oci_glue_handle->connection_handle = *connection_handle;
	oci_glue_handle->envhp	= (OCIEnv *) 0;
	oci_glue_handle->errhp	= (OCIError *) 0;
	oci_glue_handle->authp	= (OCISession *) 0;
	oci_glue_handle->srvhp	= (OCIServer *) 0;
	oci_glue_handle->svchp	= (OCISvcCtx *) 0;
	oci_glue_handle->stmthp	= (OCIStmt *) 0;
	oci_glue_handle->txnhp	= (OCITrans *) 0;

	conlist = g_slist_append (conlist, oci_glue_handle);

	*status = OCIEnvCreate(&(oci_glue_handle->envhp), OCI_DEFAULT, (dvoid *)0, 
                               0, 0, 0, (size_t) 0, (dvoid **)0);
    
	if(*status != 0) {
		OciGlue_Disconnect (*connection_handle);
		*connection_handle = 0;
		return NULL;
	}

	*status = OCIHandleAlloc( (dvoid *) (oci_glue_handle->envhp), 
					(dvoid **) &(oci_glue_handle->errhp), 
					OCI_HTYPE_ERROR, 
                   (size_t) 0, (dvoid **) 0);

	if(*status != 0) {
		OciGlue_Disconnect (*connection_handle);
		*connection_handle = 0;
		return NULL;
	}

  /* server contexts */
  *status = OCIHandleAlloc( (dvoid *) (oci_glue_handle->envhp), 
					(dvoid **) &(oci_glue_handle->srvhp), OCI_HTYPE_SERVER,
                   (size_t) 0, (dvoid **) 0);

	if(*status != 0) {
		OciGlue_Disconnect (*connection_handle);
		*connection_handle = 0;
		return NULL;
	}

  *status = OCIHandleAlloc( (dvoid *) (oci_glue_handle->envhp), 
					(dvoid **) &(oci_glue_handle->svchp), OCI_HTYPE_SVCCTX,
                   (size_t) 0, (dvoid **) 0);

	if(*status != 0) {
		OciGlue_Disconnect (*connection_handle);
		*connection_handle = 0;
		return NULL;
	}

  *status = OCIServerAttach(oci_glue_handle->srvhp, 
					oci_glue_handle->errhp, (text *) database, strlen (database), 0);

	if(*status != 0) {
		errbuf = check_error_internal (oci_glue_handle, errcode);
		OciGlue_Disconnect (*connection_handle);
		*connection_handle = 0;
		return errbuf;
	}

  *status = OCIAttrSet( (dvoid *) (oci_glue_handle->svchp), OCI_HTYPE_SVCCTX, 
					(dvoid *) (oci_glue_handle->srvhp), 
                    (ub4) 0, OCI_ATTR_SERVER, 
					(OCIError *) (oci_glue_handle->errhp));

	if(*status != 0) {
		errbuf = check_error_internal (oci_glue_handle, errcode);
		OciGlue_Disconnect (*connection_handle);
		*connection_handle = 0;
		return errbuf;
	}

  *status = OCIHandleAlloc((dvoid *) (oci_glue_handle->envhp), 
						(dvoid **)&(oci_glue_handle->authp), 
                        (ub4) OCI_HTYPE_SESSION, (size_t) 0, (dvoid **) 0);

	if(*status != 0) {
		OciGlue_Disconnect (*connection_handle);
		*connection_handle = 0;
		return NULL;
	}

  *status = OCIAttrSet((dvoid *) oci_glue_handle->authp, (ub4) OCI_HTYPE_SESSION,
                 (dvoid *) username, (ub4) strlen((char *)username),
                 (ub4) OCI_ATTR_USERNAME, oci_glue_handle->errhp);
  
	if(*status != 0) {
		errbuf = check_error_internal (oci_glue_handle, errcode);
		OciGlue_Disconnect (*connection_handle);
		*connection_handle = 0;
		return errbuf;
	}

  *status = OCIAttrSet((dvoid *) oci_glue_handle->authp, (ub4) OCI_HTYPE_SESSION,
                 (dvoid *) password, (ub4) strlen((char *)password),
                 (ub4) OCI_ATTR_PASSWORD, oci_glue_handle->errhp);

	if(*status != 0) {
		errbuf = check_error_internal (oci_glue_handle, errcode);
		OciGlue_Disconnect (*connection_handle);
		*connection_handle = 0;
		return errbuf;
	}

  *status = OCISessionBegin ( oci_glue_handle->svchp,  
						oci_glue_handle->errhp, 
						oci_glue_handle->authp, 
						OCI_CRED_RDBMS, 
						(ub4) OCI_DEFAULT);

	if(*status != 0) {
		errbuf = check_error_internal (oci_glue_handle, errcode);
		OciGlue_Disconnect (*connection_handle);
		*connection_handle = 0;
		return errbuf;
	}

  *status = OCIAttrSet((dvoid *) (oci_glue_handle->svchp), 
					(ub4) OCI_HTYPE_SVCCTX,
                   (dvoid *) (oci_glue_handle->authp), (ub4) 0,
                   (ub4) OCI_ATTR_SESSION, 
				   oci_glue_handle->errhp); 

  	if(*status != 0) {
		errbuf = check_error_internal (oci_glue_handle, errcode);
		OciGlue_Disconnect (*connection_handle);
		*connection_handle = 0;
		return errbuf;
	}

	return errbuf;
}

sword OciGlue_BeginTransaction (ub4 connection_handle)
{
	sword status = 0;
	oci_glue_connection_t *oci_glue_handle;
	oci_glue_handle = find_connection (connection_handle);

	if(!oci_glue_handle)
		return -1;

	/* Allocate transaction handle */

	status = OCIHandleAlloc ((dvoid *) oci_glue_handle->envhp,
			(dvoid **) &(oci_glue_handle->txnhp),
			(ub4) OCI_HTYPE_TRANS,
			(size_t) 0,
			(dvoid **) 0);
	if (status != 0) 
		return status;

	/* Attach the transaction to the service context */
	status = OCIAttrSet ((dvoid *) oci_glue_handle->svchp,
			OCI_HTYPE_SVCCTX,
			(dvoid *) oci_glue_handle->txnhp,
			0,
			OCI_ATTR_TRANS,
			oci_glue_handle->errhp);

	if (status != 0) {
		OCIHandleFree ((dvoid *) oci_glue_handle->txnhp, OCI_HTYPE_TRANS);
		return status;
	}

	/* Start the transaction */
	status = OCITransStart (oci_glue_handle->svchp,
			oci_glue_handle->errhp,
			60,
			OCI_TRANS_NEW);

	if (status != 0) {
		OCIHandleFree ((dvoid *) oci_glue_handle->txnhp, OCI_HTYPE_TRANS);
		return status;
	}

	return status;
}

sword OciGlue_CommitTransaction (ub4 connection_handle)
{
	sword status = 0;
	oci_glue_connection_t *oci_glue_handle;
	oci_glue_handle = find_connection (connection_handle);

	if(!oci_glue_handle)
		return -1;

	/* Attach the transaction to the service context */
	status = OCIAttrSet ((dvoid *) oci_glue_handle->svchp,
			OCI_HTYPE_SVCCTX,
			(dvoid *) oci_glue_handle->txnhp,
			0,
			OCI_ATTR_TRANS,
			oci_glue_handle->errhp);

	if (status != 0)
		return status;

	status = OCITransPrepare (oci_glue_handle->svchp,
			oci_glue_handle->errhp,
			(ub4) 0);

	if (status != 0)
		return status;

	status = OCITransCommit (oci_glue_handle->svchp,
			oci_glue_handle->errhp,
			OCI_DEFAULT);

	if (status != 0)
		return status;

	OCIHandleFree ((dvoid *) oci_glue_handle->txnhp, OCI_HTYPE_TRANS);

	return status;
}


sword OciGlue_RollbackTransaction (ub4 connection_handle)
{
	sword status = 0;
	oci_glue_connection_t *oci_glue_handle;
	oci_glue_handle = find_connection (connection_handle);

	if(!oci_glue_handle)
		return -1;

	/* Attach the transaction to the service context */
	status = OCIAttrSet ((dvoid *) oci_glue_handle->svchp,
			OCI_HTYPE_SVCCTX,
			(dvoid *) oci_glue_handle->txnhp,
			0,
			OCI_ATTR_TRANS,
			oci_glue_handle->errhp);

	if (status != 0)
		return status;

	status = OCITransRollback (oci_glue_handle->svchp,
			oci_glue_handle->errhp,
			OCI_DEFAULT);

	if (status != 0)
		return status;

	OCIHandleFree ((dvoid *) oci_glue_handle->txnhp, OCI_HTYPE_TRANS);

	return status;
}


sword OciGlue_PrepareAndExecuteNonQuerySimple (ub4 connection_handle, text *sqlstmt, ub4 *found)
{
	sword status = 0;
	oci_glue_connection_t *oci_glue_handle;
	void *node;
	GSList *con_node;
		
	if (!conlist)
		return -1;
	if (connection_handle == 0)
		return -2;
	if (!found)
		return -1;

	*found = 0;

	oci_glue_handle = find_connection (connection_handle);

	if (!oci_glue_handle)
		return -1;
	else
		*found = oci_glue_handle->connection_handle;

	if (oci_glue_handle->txnhp) {
		/* Attach a transaction */
		status = OCIAttrSet ((dvoid *) oci_glue_handle->svchp,
				OCI_HTYPE_SVCCTX,
				(dvoid *) oci_glue_handle->txnhp,
				0,
				OCI_ATTR_TRANS,
				oci_glue_handle->errhp);

		if (status != 0)
			return status;
	}

	if (!(oci_glue_handle->stmthp)) {
		status = OCIHandleAlloc ((dvoid *) oci_glue_handle->envhp, 
				(dvoid **) &(oci_glue_handle->stmthp),
				(ub4) OCI_HTYPE_STMT, 
				(CONST size_t) 0, 
				(dvoid **) 0);

		if(status != 0)
			return status;
	}

	/* Prepare the statement for execution */
	status = OCIStmtPrepare ((oci_glue_handle->stmthp), 
				oci_glue_handle->errhp, 
				(CONST OraText *)sqlstmt, 
				(ub4) strlen (sqlstmt),
				(ub4) OCI_NTV_SYNTAX, 
				(ub4) OCI_DEFAULT);

	if (status != 0)
		return status;

	/* Execute the statement */
	status = OCIStmtExecute (oci_glue_handle->svchp, 
				oci_glue_handle->stmthp, 
				oci_glue_handle->errhp, 
				(ub4) 1,
				(ub4) 0, 
				(CONST OCISnapshot *) NULL, 
				(OCISnapshot *) NULL, 
				OCI_DEFAULT);

	return status;
}

sword OciGlue_Disconnect (ub4 connection_handle)
{
	sword status = -1;
	
	GSList *node = NULL;
	oci_glue_connection_t *oci_glue_handle = NULL;

	if(connection_handle == 0)
		return -2;

	if(conlist == NULL)
		return -2;

	oci_glue_handle = find_connection (connection_handle);

	if(oci_glue_handle) {
		
		if(oci_glue_handle->svchp && oci_glue_handle->errhp && oci_glue_handle->authp)
			status = OCISessionEnd(oci_glue_handle->svchp, 
				oci_glue_handle->errhp, oci_glue_handle->authp, (ub4) 0);
  
		if(oci_glue_handle->srvhp && oci_glue_handle->errhp)
			status = OCIServerDetach(oci_glue_handle->srvhp, oci_glue_handle->errhp, 
				(ub4) OCI_DEFAULT);

		if (oci_glue_handle->srvhp)
			status = OCIHandleFree((dvoid *) oci_glue_handle->srvhp, (ub4) OCI_HTYPE_SERVER);

		if (oci_glue_handle->svchp)
			status = OCIHandleFree((dvoid *) oci_glue_handle->svchp, (ub4) OCI_HTYPE_SVCCTX);

		if (oci_glue_handle->errhp)
			status = OCIHandleFree((dvoid *) oci_glue_handle->errhp, (ub4) OCI_HTYPE_ERROR);

		if (oci_glue_handle->authp)
			status = OCIHandleFree((dvoid *) oci_glue_handle->authp, (ub4) OCI_HTYPE_SESSION);

		if (oci_glue_handle->stmthp)
			status = OCIHandleFree((dvoid *) oci_glue_handle->stmthp, (ub4) OCI_HTYPE_STMT);

		if (oci_glue_handle->envhp)
			status = OCIHandleFree((dvoid *) oci_glue_handle->envhp, (ub4) OCI_HTYPE_ENV);

	}
	node = find_connection_node (connection_handle);
	if(node) {
			conlist = g_slist_remove_link (conlist, node);
			g_slist_free_1 (node);
			node = NULL;
	}
	if(oci_glue_handle) {
		g_free (oci_glue_handle);
		oci_glue_handle = NULL;
	}

	status = 0;
	
  return status;
}

guint OciGlue_ConnectionCount ()
{
	return g_slist_length (conlist);
}

text *OciGlue_CheckError (sword status, ub4 connection_handle)
{
	oci_glue_connection_t *oci_glue_handle = NULL;
	text *errbuf;
	sb4 errcode = 0;
	size_t errbuf_size;
	
	if(!conlist)
		return NULL;

	if(connection_handle == 0)
		return NULL;

	oci_glue_handle = find_connection (connection_handle);

	if(!oci_glue_handle)
		return NULL;

	errbuf_size = sizeof(text) * 512;
	errbuf = (text *) g_malloc(errbuf_size);

	OCIErrorGet((dvoid *)(oci_glue_handle->errhp), 
		(ub4) 1, (text *) NULL, &errcode,
		errbuf, (ub4) errbuf_size, OCI_HTYPE_ERROR);
	
	return errbuf;
}

void OciGlue_Free (void *obj)
{
	g_free(obj);
}

/* ------------- Private Functions ----------- */

text *check_error_internal (oci_glue_connection_t *oci_glue_handle, sb4 *errcode)
{
	text* errbuf = NULL;
	size_t errbuf_size;

	if(!oci_glue_handle)
		return NULL;
	if(!errcode)
		return NULL;

	errbuf_size = sizeof(text) * 512;
	errbuf = (text *) g_malloc(errbuf_size);

	OCIErrorGet((dvoid *)(oci_glue_handle->errhp), 
		(ub4) 1, (text *) NULL, errcode,
		errbuf, (ub4) errbuf_size, OCI_HTYPE_ERROR);

	return errbuf;
}

GSList *find_connection_node (ub4 connection_handle)
{
	GSList *node = NULL;
	oci_glue_connection_t *oci_glue_handle = NULL;

	for(node = conlist;
		node;
		node = node->next) {

		oci_glue_handle = (oci_glue_connection_t *) node->data;
		if(oci_glue_handle->connection_handle == connection_handle)
			return node;
	}
	return NULL;
}

oci_glue_connection_t *find_connection (ub4 connection_handle)
{	
	GSList *node = NULL;
	oci_glue_connection_t *oci_glue_handle = NULL;

	for(node = conlist;
		node;
		node = node->next) {

		oci_glue_handle = (oci_glue_connection_t *) node->data;	
		if(oci_glue_handle->connection_handle == connection_handle)
			return oci_glue_handle;		
	}
	return NULL;
}
