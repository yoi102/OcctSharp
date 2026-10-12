#include "OcctSharp.Modeling.ModelingResults.Generated.h"
#include "OcctSharp.Modeling.TopologyExchange.Generated.h"
#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#define require(condition) do { if(!(condition)){fprintf(stderr,"Modeling ABI assertion failed at line %d\n",__LINE__);exit(1);} } while(0)
int main(void){
 OcctSharp_ShapeHandle* box=NULL;require(occtsharp_shape_create_box(2,3,4,&box)==OCCTSHARP_STATUS_SUCCESS);
 const OcctSharp_ShapeHandle* inputs[2]={box,box};void* set=NULL;require(occtsharp_shape_set_create(inputs,2,&set)==OCCTSHARP_STATUS_SUCCESS);
 void* token=(void*)1;require(occtsharp_modeling_result_section(set,0,1,1,NAN,&token)==OCCTSHARP_STATUS_INVALID_ARGUMENT&&token==NULL);
 require(occtsharp_modeling_result_section(set,0,1,1,1e-7,&token)==OCCTSHARP_STATUS_SUCCESS&&token!=NULL);
 int32_t links=0;require(occtsharp_modeling_result_counts(token,&links)==OCCTSHARP_STATUS_SUCCESS&&links>=0&&links<=65536);
 int32_t info[20];double config[8];int32_t* rows=(int32_t*)malloc((size_t)(links*7+2)*sizeof(int32_t));require(rows!=NULL);
 for(int i=0;i<20;i++)info[i]=123456;for(int i=0;i<8;i++)config[i]=123456;for(int i=0;i<links*7+2;i++)rows[i]=123456;
 require(occtsharp_modeling_result_snapshot(token,info,17,config,6,rows,links*7)==OCCTSHARP_STATUS_INVALID_ARGUMENT);
 for(int i=0;i<20;i++)require(info[i]==123456);for(int i=0;i<8;i++)require(config[i]==123456);for(int i=0;i<links*7+2;i++)require(rows[i]==123456);
 if(links>0){require(occtsharp_modeling_result_snapshot(token,info,18,config,6,rows,links*7-1)==OCCTSHARP_STATUS_INVALID_ARGUMENT);for(int i=0;i<20;i++)require(info[i]==123456);for(int i=0;i<8;i++)require(config[i]==123456);}
 require(occtsharp_modeling_result_snapshot(token,info,18,config,6,rows,links*7)==OCCTSHARP_STATUS_SUCCESS);
 require(info[0]==1&&info[3]==2&&info[5]==links&&info[18]==123456&&config[6]==123456&&rows[links*7]==123456);
 OcctSharp_ShapeHandle* copied=NULL;require(occtsharp_modeling_result_shape(token,-1,0,&copied)==OCCTSHARP_STATUS_SUCCESS&&copied!=NULL);
 /* Typed registry Acquire uses INVALID_ARGUMENT for stale/wrong-type owned tokens. */
 occtsharp_modeling_result_release(token);int32_t untouched=123456;require(occtsharp_modeling_result_counts(token,&untouched)==OCCTSHARP_STATUS_INVALID_ARGUMENT&&untouched==123456);
 occtsharp_shape_release(copied);occtsharp_shape_release(box);occtsharp_topology_collection_release(set,0);free(rows);
 puts("Modeling result buffer ABI PASS: invalid configuration, atomic short-buffer canaries, successful aggregate publication, copied topology and released-token rejection.");return 0;
}
