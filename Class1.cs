
/** JSONH.pack for ASP.NET
 * @description JSON Homogeneous Collection Packer
 * @version     1.0.1
 * @author      Andrea Giammarchi
 * @license     Mit Style License
 * @project     http://github.com/WebReflection/json.hpack/tree/master
 * @blog        http://webreflection.blogspot.com/
 */

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Script.Serialization;

 public static class    Help
 {
     public static T[][] init2D<T>(int i, int i1)
     {

         T[][] j = new T[i][];

         for (int k = 0; k < i1; k++)
         {
             j[k] = new T[i1];
         }
         return j;
     }
    
 
 }