#include "OcctSharp.Modeling.ParameterizedConstruction.Generated.h"
#include <assert.h>
#include <stdint.h>
#include <stdio.h>
int main(void){
 OcctSharp_Geom2dCurveHandle* curve=(OcctSharp_Geom2dCurveHandle*)(uintptr_t)1;
 OcctSharp_GeomSurfaceHandle* surface=(OcctSharp_GeomSurfaceHandle*)(uintptr_t)1;
 assert(occtsharp_parameterized_copy_spline_pcurve(NULL,NULL)!=OCCTSHARP_STATUS_SUCCESS);
 assert(occtsharp_parameterized_copy_spline_surface(NULL,NULL)!=OCCTSHARP_STATUS_SUCCESS);
 assert(occtsharp_parameterized_copy_spline_pcurve(NULL,&curve)!=OCCTSHARP_STATUS_SUCCESS&&curve==NULL);
 assert(occtsharp_parameterized_copy_spline_surface(NULL,&surface)!=OCCTSHARP_STATUS_SUCCESS&&surface==NULL);
 curve=(OcctSharp_Geom2dCurveHandle*)(uintptr_t)1;surface=(OcctSharp_GeomSurfaceHandle*)(uintptr_t)1;
 assert(occtsharp_parameterized_copy_spline_pcurve((const OcctSharp_Geom2dBSplineCurveHandle*)(uintptr_t)1,&curve)!=OCCTSHARP_STATUS_SUCCESS&&curve==NULL);
 assert(occtsharp_parameterized_copy_spline_surface((const OcctSharp_GeomBSplineSurfaceHandle*)(uintptr_t)1,&surface)!=OCCTSHARP_STATUS_SUCCESS&&surface==NULL);
 puts("Typed topology C ABI PASS: absent outputs, null/stale unregistered sources, cleared failed publications.");return 0;
}
