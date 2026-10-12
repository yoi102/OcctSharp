#include "OcctSharp.Xde.LocalStepExport.Generated.h"
#include "OcctSharp.Documents.DocumentProjections.Generated.h"
#include <assert.h>
#include <direct.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
int main(int argc,char** argv){
 /* Raw legacy document ABI reads a concrete resident part; new multifile assemblies are validated through BJ in both managed consumers. */
 assert(argc==2);OcctSharp_OcafDocumentHandle* doc=NULL;assert(occtsharp_xde_document_read_step("tests/Fixtures/LocalStep/piece_part.stp",&doc)==OCCTSHARP_STATUS_SUCCESS&&doc);
 char entry[1024];int32_t written=0;assert(occtsharp_xde_document_free_shape_entry(doc,1,entry,1024,&written)==OCCTSHARP_STATUS_SUCCESS&&written>0&&written<1024);
 uint8_t input[1044];int32_t fields[5]={1,2,81,1,written};memcpy(input,fields,20);memcpy(input+20,entry,(size_t)written);void* token=(void*)(uintptr_t)1;void* snapshot=(void*)(uintptr_t)1;
 assert(occtsharp_local_step_export_prepare(doc,1,input,20+written,9,7,1e-4,&token,&snapshot)!=OCCTSHARP_STATUS_SUCCESS&&token==NULL&&snapshot==NULL);
 token=(void*)(uintptr_t)1;assert(occtsharp_local_step_export_prepare(doc,1,input,20+written,4,7,1e-4,&token,&token)!=OCCTSHARP_STATUS_SUCCESS&&token==NULL);
 assert(occtsharp_local_step_export_prepare(doc,1,input,20+written-1,4,7,1e-4,&token,&snapshot)!=OCCTSHARP_STATUS_SUCCESS&&token==NULL&&snapshot==NULL);
 OcctSharp_Status prepared=occtsharp_local_step_export_prepare(doc,1,input,20+written,4,7,1e-4,&token,&snapshot);if(prepared!=OCCTSHARP_STATUS_SUCCESS)fprintf(stderr,"Export preparation failed: %s\n",occtsharp_get_last_error());assert(prepared==OCCTSHARP_STATUS_SUCCESS&&token&&snapshot);occtsharp_ocaf_document_release(doc);
 int32_t bytes=0;assert(occtsharp_document_projection_payload_size(snapshot,2,&bytes)==OCCTSHARP_STATUS_SUCCESS&&bytes>32&&bytes<1048576);uint8_t* data=(uint8_t*)malloc((size_t)bytes+2);assert(data);memset(data,0x55,(size_t)bytes+2);
 assert(occtsharp_document_projection_payload_copy(snapshot,2,data+1,bytes-1)!=OCCTSHARP_STATUS_SUCCESS);for(int32_t i=0;i<bytes+2;++i)assert(data[i]==0x55);
 assert(occtsharp_document_projection_payload_copy(snapshot,2,data+1,bytes)==OCCTSHARP_STATUS_SUCCESS&&data[0]==0x55&&data[bytes+1]==0x55);int32_t header[5];memcpy(header,data+1,20);assert(header[0]==1&&header[1]==2&&header[2]==80&&header[3]==4&&header[4]==7);free(data);occtsharp_document_projection_payload_release(snapshot,2);
 assert(occtsharp_document_projection_payload_size(snapshot,2,&bytes)!=OCCTSHARP_STATUS_SUCCESS);
 assert(_mkdir(argv[1])==0);assert(occtsharp_local_step_export_write(token,argv[1])==OCCTSHARP_STATUS_SUCCESS);assert(occtsharp_local_step_export_write(token,argv[1])!=OCCTSHARP_STATUS_SUCCESS);occtsharp_local_step_export_release(token);assert(occtsharp_local_step_export_write(token,argv[1])!=OCCTSHARP_STATUS_SUCCESS);
 puts("Local STEP export C ABI PASS: cleared/aliased failed outputs, bounded snapshot canaries, private canonical staging, source closure, consumed epoch and stale tokens.");return 0;
}
