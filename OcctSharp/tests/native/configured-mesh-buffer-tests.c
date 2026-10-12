#include "OcctSharp.Xde.MeshExchangeConfig.Generated.h"
#include "OcctSharp.Xde.DocumentAssociations.Generated.h"
#include "OcctSharp.Documents.DocumentProjections.Generated.h"
#include "OcctSharp.Documents.SharedHandles.Generated.h"
#include <assert.h>
#include <stdint.h>
#include <stdlib.h>
#include <string.h>
#include <stdio.h>

int main(void){
 void* document=NULL;void* token=(void*)(uintptr_t)1;
 assert(occtsharp_document_associations_create_native(&document)==OCCTSHARP_STATUS_SUCCESS&&document);
 assert(occtsharp_mesh_common_snapshot(document,0,"0:1",&token)==OCCTSHARP_STATUS_SUCCESS&&token);
 int32_t length=0;assert(occtsharp_document_projection_payload_size(token,2,&length)==OCCTSHARP_STATUS_SUCCESS&&length>4);
 uint8_t* bytes=malloc((size_t)length+2);assert(bytes);memset(bytes,0xAB,(size_t)length+2);
 assert(occtsharp_document_projection_payload_copy(token,2,bytes+1,length-1)!=OCCTSHARP_STATUS_SUCCESS);
 for(int32_t n=0;n<length+2;++n)assert(bytes[n]==0xAB);
 assert(occtsharp_document_projection_payload_copy(token,2,bytes+1,length)==OCCTSHARP_STATUS_SUCCESS&&bytes[0]==0xAB&&bytes[length+1]==0xAB);
 occtsharp_document_projection_payload_release(token,2);
 assert(occtsharp_document_projection_payload_size(token,2,&length)!=OCCTSHARP_STATUS_SUCCESS);
 token=(void*)(uintptr_t)1;assert(occtsharp_mesh_common_snapshot(document,0,"0:999999",&token)!=OCCTSHARP_STATUS_SUCCESS&&token==NULL);
 token=(void*)(uintptr_t)1;assert(occtsharp_mesh_config_write(document,0,"unused.gltf",-1,NULL,0,&token)!=OCCTSHARP_STATUS_SUCCESS&&token==NULL);
 void* owner=(void*)(uintptr_t)1;void* geometry=(void*)(uintptr_t)1;token=(void*)(uintptr_t)1;
 assert(occtsharp_mesh_config_read("absent.gltf",-1,NULL,0,&owner,&geometry,&token)!=OCCTSHARP_STATUS_SUCCESS&&owner==NULL&&geometry==NULL&&token==NULL);
 assert(occtsharp_mesh_common_replace(document,0,"0:1",NULL,0)!=OCCTSHARP_STATUS_SUCCESS);
 assert(occtsharp_mesh_definition_replace(document,0,"0:1",NULL)!=OCCTSHARP_STATUS_SUCCESS);
 occtsharp_generated_tdoc_std_document_release((OcctSharp_TDocStdDocumentHandle*)document);free(bytes);
 puts("Configured mesh C ABI PASS: copied payload canaries, stale tokens, material/definition guards and cleared failed writer/reader outputs.");return 0;
}
