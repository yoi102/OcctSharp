#include "OcctSharp.Documents.DirectCollections.Generated.h"
#include "OcctSharp.Documents.DocumentProjections.Generated.h"
#include "OcctSharp.Documents.SharedHandles.Generated.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#define require(c) do{if(!(c)){fprintf(stderr,"Direct ABI assertion failed at %d\n",__LINE__);exit(1);}}while(0)
static uint8_t input[256];static int length;
static void number(int32_t value){memcpy(input+length,&value,4);length+=4;}
static void text(const char* value){int n=(int)strlen(value);number(n);memcpy(input+length,value,(size_t)n);length+=n;}
int main(void){
 void* document=NULL;require(occtsharp_document_projection_create_native(&document)==OCCTSHARP_STATUS_SUCCESS&&document!=NULL);
 number(1);number(1);number(61);number(1);text("0:1");number(0);text("");number(1);number(-3);number(3);number(4);number(0);number(7);
 require(occtsharp_direct_collection_replace(document,0,input,length)==OCCTSHARP_STATUS_INVALID_ARGUMENT);
 require(occtsharp_generated_tdoc_std_document_method_new_command_0((const OcctSharp_TDocStdDocumentHandle*)document)==OCCTSHARP_STATUS_SUCCESS);
 require(occtsharp_direct_collection_replace(document,0,input,length)==OCCTSHARP_STATUS_SUCCESS);
 void* payload=NULL;require(occtsharp_direct_collection_snapshot(document,0,"0:1",0,"",&payload)==OCCTSHARP_STATUS_SUCCESS&&payload!=NULL);
 int32_t count=0;require(occtsharp_document_projection_payload_size(payload,1,&count)==OCCTSHARP_STATUS_SUCCESS&&count>32&&count<256);
 uint8_t output[258];memset(output,0xCD,sizeof(output));require(occtsharp_document_projection_payload_copy(payload,1,output,count-1)==OCCTSHARP_STATUS_INVALID_ARGUMENT);for(int n=0;n<258;++n)require(output[n]==0xCD);
 require(occtsharp_document_projection_payload_copy(payload,1,output,count)==OCCTSHARP_STATUS_SUCCESS&&output[count]==0xCD);
 int32_t actualLower=0,actualCount=0,first=0,last=0;memcpy(&actualLower,output+count-20,4);memcpy(&actualCount,output+count-16,4);memcpy(&first,output+count-12,4);memcpy(&last,output+count-4,4);require(actualLower==-3&&actualCount==3&&first==4&&last==7);
 /* Truncation and invalid GUIDs cannot remove the existing direct attribute. */
 require(occtsharp_direct_collection_replace(document,0,input,length-1)==OCCTSHARP_STATUS_INVALID_ARGUMENT);
 void* failed=(void*)1;require(occtsharp_direct_collection_snapshot(document,0,"0:1",0,"not-a-guid",&failed)!=OCCTSHARP_STATUS_SUCCESS&&failed==NULL);
 occtsharp_document_projection_payload_release(payload,1);memset(output,0xCD,sizeof(output));require(occtsharp_document_projection_payload_copy(payload,1,output,count)==OCCTSHARP_STATUS_INVALID_ARGUMENT);for(int n=0;n<258;++n)require(output[n]==0xCD);
 require(occtsharp_generated_tdoc_std_document_method_abort_command_0((const OcctSharp_TDocStdDocumentHandle*)document)==OCCTSHARP_STATUS_SUCCESS);
 require(occtsharp_direct_collection_snapshot(document,0,"0:1",0,"",&payload)==OCCTSHARP_STATUS_SUCCESS);require(occtsharp_document_projection_payload_size(payload,1,&count)==OCCTSHARP_STATUS_SUCCESS&&count==20);occtsharp_document_projection_payload_release(payload,1);
 occtsharp_generated_tdoc_std_document_release((OcctSharp_TDocStdDocumentHandle*)document);
 puts("Direct collection C ABI PASS: caller command, bounded typed snapshot/bounds, short-buffer canaries, truncated/GUID failure publication, stale payload and abort restoration.");return 0;
}
