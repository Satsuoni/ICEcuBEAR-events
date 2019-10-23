csv=require("csv")
const parse = require('csv-parse/lib/sync')
dela=require("d3-geo-voronoi")
fs=require("fs")
fl=fs.readFileSync('stars.csv')
rec=parse(fl,{ columns: true, skip_empty_lines: true })
arr=Array()
flux=Array()
for(var n in rec)
{
    if(typeof rec[n].RAJ2000 !== 'undefined' && typeof rec[n].DEJ2000!== 'undefined')
    {
    arr.push([parseFloat(rec[n].RAJ2000),parseFloat(rec[n].DEJ2000)]);
    flux.push(Math.log(parseFloat(rec[n].Flux1000)));
 }
}
//console.log(arr[0][0])
del=dela.geoDelaunay(arr)
tri=del.triangles

ret=Object()
ret.uv=Array()
ret.uv2=Array()
ret.uv3=Array()
ret.uv4=Array()
ret.uv5=Array()
ret.uv6=Array()
ret.vertices=Array()
ret.normals=Array()
ret.triangles=Array()

cver=0
for(var n in tri)
{
    t=tri[n];
    ret.triangles.push(cver,cver+1,cver+2);
    v0_pos=dela.cartesian(arr[t[0]]);
    v1_pos=dela.cartesian(arr[t[1]]);
    v2_pos=dela.cartesian(arr[t[2]]);
    ret.vertices.push(v0_pos,v1_pos,v2_pos);
    ret.normals.push(v0_pos,v1_pos,v2_pos);
    v0red=[v0_pos[0],(v0_pos[2]+1)/4.0];
    if (v0_pos[1]<0)v0red[1]= v0red[1]+0.5;
    v1red=[v1_pos[0],(v1_pos[2]+1)/4.0];
    if (v1_pos[1]<0)v1red[1]= v1red[1]+0.5;
    v2red=[v2_pos[0],(v2_pos[2]+1)/4.0];
    if (v2_pos[1]<0)v2red[1]= v2red[1]+0.5;
    ret.uv.push(v0red,v0red,v0red);
    ret.uv2.push(v1red,v1red,v1red);
    ret.uv3.push(v2red,v2red,v2red);
    ret.uv4.push([flux[t[0]],0]);
    ret.uv5.push([flux[t[1]],0]);
    ret.uv6.push([flux[t[2]],0]);
    cver=cver+3
}
//console.log(del.triangles.length)
console.log(JSON.stringify(ret))
console.log(dela.cartesian([0,90]))
