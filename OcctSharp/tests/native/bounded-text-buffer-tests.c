#include "OcctSharp.Foundation.BoundedTextTransforms.Generated.h"
#include <assert.h>
#include <stdint.h>
#include <stdio.h>
#include <string.h>
int main(void){
 uint16_t input[]={0x4E2D,0xD83D,0xDE80,0x6587},other[]={0x6C49,0x5B57};void* token=(void*)(uintptr_t)1;int32_t bytes=9,units=9;
 assert(occtsharp_bounded_text_compute(1,3,input,4,other,1,2,32,&token,&bytes,&units)!=OCCTSHARP_STATUS_SUCCESS&&token==NULL&&bytes==0&&units==0);
 assert(occtsharp_bounded_text_compute(1,3,input,4,other,2,2,32,&token,&bytes,&units)==OCCTSHARP_STATUS_SUCCESS&&token&&bytes==12&&units==4);
 char data[15];memset(data,0x55,sizeof(data));assert(occtsharp_bounded_text_copy(token,data+1,bytes)!=OCCTSHARP_STATUS_SUCCESS);for(int i=0;i<15;++i)assert(data[i]==0x55);
 assert(occtsharp_bounded_text_copy(token,data+1,bytes+1)==OCCTSHARP_STATUS_SUCCESS&&data[0]==0x55&&data[14]==0x55&&data[13]==0);
 occtsharp_bounded_text_release(token);assert(occtsharp_bounded_text_copy(token,data+1,bytes+1)!=OCCTSHARP_STATUS_SUCCESS);
 uint16_t invalid[]={0xD800},nul[]={0};token=(void*)(uintptr_t)1;bytes=9;units=9;
 assert(occtsharp_bounded_text_compute(1,1,invalid,1,NULL,0,1,32,&token,&bytes,&units)!=OCCTSHARP_STATUS_SUCCESS&&token==NULL&&bytes==0&&units==0);
 assert(occtsharp_bounded_text_compute(1,1,nul,1,NULL,0,1,32,&token,&bytes,&units)!=OCCTSHARP_STATUS_SUCCESS&&token==NULL);
 assert(occtsharp_bounded_text_compute(0,1,input,4,NULL,0,1,32,&token,&bytes,&units)!=OCCTSHARP_STATUS_SUCCESS&&token==NULL);
 assert(occtsharp_bounded_text_compute(1,1,NULL,0,NULL,0,1,32,&token,&bytes,&units)==OCCTSHARP_STATUS_SUCCESS&&bytes==0&&units==0);
 char empty[3]={0x55,0x55,0x55};assert(occtsharp_bounded_text_copy(token,empty+1,1)==OCCTSHARP_STATUS_SUCCESS&&empty[0]==0x55&&empty[1]==0&&empty[2]==0x55);occtsharp_bounded_text_release(token);
 puts("Bounded text C ABI PASS: explicit lengths, UTF16 boundaries, UTF8 copy canaries, empty output, stale tokens and cleared failed outputs.");return 0;
}
