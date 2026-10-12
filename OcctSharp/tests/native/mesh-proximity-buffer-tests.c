#include "OcctSharp.Mesh.MeshProximity.Generated.h"
#include "OcctSharp.Mesh.MeshingWorkspaces.Generated.h"
#include "OcctSharp.Modeling.ParameterizedConstruction.Generated.h"
#include <assert.h>
#include <stdint.h>
#include <stdio.h>
static OcctSharp_ShapeHandle* face(double z){
 double data[11]={0,0,0,0,0,1,1,0,0,1,1},bounds[4]={0,4,0,4};data[2]=z;
 void* construction=NULL;void* mesh=NULL;OcctSharp_ShapeHandle* source=NULL;OcctSharp_ShapeHandle* result=NULL;
 assert(occtsharp_parameterized_face(0,data,11,bounds,4,&construction)==OCCTSHARP_STATUS_SUCCESS);
 assert(occtsharp_parameterized_shape(construction,0,&source)==OCCTSHARP_STATUS_SUCCESS);
 double values[5]={.5,.1,-1,-1,-1};int32_t flags[10]={0,0,0,0,0,0,0,0,0,0};
 assert(occtsharp_meshing_create(source,values,5,flags,10,&mesh)==OCCTSHARP_STATUS_SUCCESS);
 assert(occtsharp_meshing_shape(mesh,&result)==OCCTSHARP_STATUS_SUCCESS);
 occtsharp_meshing_release(mesh);occtsharp_shape_release(source);occtsharp_parameterized_release(construction);return result;
}
int main(void){
 OcctSharp_ShapeHandle* a=face(0);OcctSharp_ShapeHandle* b=face(2);void* result=(void*)(uintptr_t)1;
 assert(occtsharp_mesh_proximity_compute(a,b,2,0,0,0,256,100000,100000,65536,100000,&result)!=OCCTSHARP_STATUS_SUCCESS&&result==NULL);
 assert(occtsharp_mesh_proximity_compute(a,a,0,0,0,0,256,100000,100000,65536,100000,&result)==OCCTSHARP_STATUS_SUCCESS&&result);
 int32_t info[14];double values[10];for(int i=0;i<14;++i)info[i]=999;for(int i=0;i<10;++i)values[i]=777;
 assert(occtsharp_mesh_proximity_snapshot(result,info+1,11,values+1,8)!=OCCTSHARP_STATUS_SUCCESS);
 for(int i=0;i<14;++i)assert(info[i]==999);for(int i=0;i<10;++i)assert(values[i]==777);
 assert(occtsharp_mesh_proximity_snapshot(result,info+1,12,values+1,8)==OCCTSHARP_STATUS_SUCCESS&&info[0]==999&&info[13]==999&&values[0]==777&&values[9]==777&&info[5]==1&&info[10]==0);
 int32_t pair[4]={999,999,999,999};assert(occtsharp_mesh_proximity_pairs(result,pair+1,1)!=OCCTSHARP_STATUS_SUCCESS);for(int i=0;i<4;++i)assert(pair[i]==999);
 assert(occtsharp_mesh_proximity_pairs(result,pair+1,2)==OCCTSHARP_STATUS_SUCCESS&&pair[0]==999&&pair[3]==999&&pair[1]==0&&pair[2]==0);
 OcctSharp_ShapeHandle* copied=(OcctSharp_ShapeHandle*)(uintptr_t)1;assert(occtsharp_mesh_proximity_face(result,2,0,&copied)!=OCCTSHARP_STATUS_SUCCESS&&copied==NULL);
 assert(occtsharp_mesh_proximity_face(result,0,0,&copied)==OCCTSHARP_STATUS_SUCCESS&&copied);occtsharp_mesh_proximity_release(result);occtsharp_shape_release(copied);
 assert(occtsharp_mesh_proximity_snapshot(result,info+1,12,values+1,8)!=OCCTSHARP_STATUS_SUCCESS);
 assert(occtsharp_mesh_proximity_compute(a,b,1,0,0,0,256,100000,100000,65536,100000,&result)==OCCTSHARP_STATUS_SUCCESS&&result);occtsharp_shape_release(a);occtsharp_shape_release(b);
 assert(occtsharp_mesh_proximity_snapshot(result,info+1,12,values+1,8)==OCCTSHARP_STATUS_SUCCESS&&info[2]==1&&info[10]==1&&values[2]>1.999&&values[2]<2.001);
 assert(occtsharp_mesh_proximity_pairs(result,NULL,0)==OCCTSHARP_STATUS_SUCCESS);
 int32_t mesh[4]={999,999,999,999};double deflection=777;assert(occtsharp_mesh_proximity_mesh(result,0,0,mesh+1,1,&deflection)!=OCCTSHARP_STATUS_SUCCESS&&mesh[1]==999&&deflection==777);
 assert(occtsharp_mesh_proximity_mesh(result,0,0,mesh+1,2,&deflection)==OCCTSHARP_STATUS_SUCCESS&&mesh[0]==999&&mesh[3]==999&&mesh[1]==4&&mesh[2]==2);
 occtsharp_mesh_proximity_release(result);
 puts("Mesh proximity C ABI PASS: snapshot/pair/mesh canaries, completion field presence, empty pairs, independent input lifetime, stale tokens and cleared failed outputs.");return 0;
}
