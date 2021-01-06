using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Text;
using UnityEngine;
using SharpCompress.Compressors.Deflate;
using SimpleJSON;
public class Meshobject
{
    public Vector3[] verts;
    public Vector2[] uvs;
    public int[] indices;
    public Meshobject(Vector3 [] v, Vector2 [] u, int[] i)
    {
        verts = v;
        uvs = u;
        indices = i;
    }
    public Meshobject(JSONObject obj)
    {
        JSONArray vrt =(JSONArray) obj["vertices"];
        JSONArray uv = (JSONArray) obj["uvs"];
        JSONArray ind = (JSONArray)obj["indices"];
        if(vrt==null||uv==null||ind==0)
        {
            return;
        }
        verts = new Vector3[vrt.Count];
        int cnt = 0;
        foreach(JSONNode n in vrt)
        {
            if (!n.IsArray) return;
            verts[cnt] =new Vector3(n[0],n[1],n[2]);
            cnt++;
        }
        cnt = 0;
        uvs = new Vector2[uv.Count];
        foreach (JSONNode n in uv)
        {
            if (!n.IsArray) return;
            uvs[cnt] = new Vector2(n[0], n[1]);
            cnt++;
        }
        cnt = 0;

        indices = new int[ind.Count];
        foreach (JSONNode n in ind)
        {
            indices[cnt] = n;
            cnt++;
        }
    }
    public Mesh ToMesh()
    {
        Mesh newM = new Mesh();
        newM.vertices = verts;
        newM.uv = uvs;
        newM.SetIndices(indices, MeshTopology.Lines, 0);
        return newM;
    }
    public  JSONNode ToJson()
    {
        JSONObject ret = new JSONObject();
        ret["vertices"] = new JSONArray();
        ret["uvs"] = new JSONArray();
        ret["indices"] = new JSONArray();
        foreach(Vector3 v in verts)
        {
            JSONArray varr = new JSONArray();
            varr.Add(v.x); varr.Add(v.y); varr.Add(v.z);
            ret["vertices"].Add(varr);
        }
        foreach (Vector2 v in uvs)
        {
            JSONArray varr = new JSONArray();
            varr.Add(v.x); varr.Add(v.y); 
            ret["uvs"].Add(varr);
        }
        foreach (int index in indices)
        {
            ret["indices"].Add(index);
        }
            return ret;
    }
}
public class MultimeshObject
{
    public List<Meshobject> meshes=new List<Meshobject>();
    public bool SaveToFile(string fname)
    {
        JSONArray  data = new JSONArray();
        foreach(Meshobject msh in meshes)
        {
            data.Add(msh.ToJson());
        }
            using (Stream stream = File.OpenWrite(fname))
            using (var writer = new GZipStream(stream, SharpCompress.Compressors.CompressionMode.Compress, SharpCompress.Compressors.Deflate.CompressionLevel.Level5))
            {
                byte[] bt = Encoding.ASCII.GetBytes(data.ToString());
                writer.Write(bt, 0, bt.Length);
                
                // bt.CopyTo(writer);
            }
        return true;
    }
    public bool LoadFromZippedBytes(byte[] bytes)
    {
        MemoryStream msin = new MemoryStream(bytes,false);
        byte[] tmp = null;
        using (var reader = new GZipStream(msin, SharpCompress.Compressors.CompressionMode.Decompress))
        {
            using (MemoryStream ms = new MemoryStream())
            {
                reader.CopyTo(ms);
                tmp = ms.ToArray();
                Debug.LogFormat("unpacked {0}",tmp.Length);
               // return true;
               
            }

        }
        return LoadFromBytes(tmp);
    }
        public bool LoadFromBytes(byte[] bytes)
    {
        string dat = "";
        try
        {
           dat=Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return false;
        }
        JSONNode data = JSON.Parse(dat);
        if (!data.IsArray) return false;
        meshes = new List<Meshobject>();
        foreach(var jn in data)
        {
            if (!jn.Value.IsObject) return false;
            Meshobject mesh = new Meshobject((JSONObject)jn.Value);
            meshes.Add(mesh);
        }
        return true;
    }
    public bool LoadFromFile(string fname)
    {
        if (!File.Exists(fname)) return false;
        using (Stream stream = File.OpenRead(fname))
        {
            using (var reader = new GZipStream(stream, SharpCompress.Compressors.CompressionMode.Decompress))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    reader.CopyTo(ms);
                    return LoadFromBytes(ms.ToArray());
                }

            }
        }
      //  return false;
    }


}

public class ScatterPhoton
{
    public static float speed = 0.299792458f / 1.32501299335f; //per nanosecond
    public float timeOffset;
    public Vector3 position;
    public Vector3 direction;
    public bool decayed=false;
    public Vector3 prevPosition;
    public float prevTime = 0;
    public void Propagate(float absorbtion, float scatter)// scatter length
    {
        float distance = TrailmeshMaker.DrawExp(scatter);
        //Debug.LogFormat("Ran: {0} {1} {2}", distance, absorbtion, scatter);
        float dur = distance / speed;
        float decayprob = 1.0f - Mathf.Exp(-distance / absorbtion);
        prevPosition = position;
        prevTime = timeOffset;
        position = position + direction * distance;
        timeOffset += dur;
        if (TrailmeshMaker.rndgen.NextDouble()<=decayprob)
        {
            decayed = true;
            return;
        }
        direction=TrailmeshMaker.Scatter(direction);
    }
}

public static class IcecubeDust
{

    public static class Tilt
    {
        public static double[] distancesFromOriginAlongTilt = new double[] { -531.419, -454.882, 0.0, 165.202, 445.477, 520.77 };
        public static double[] zCoordinates = new double[] { -500.39999999999986, -490.39999999999986, -480.39999999999986, -470.39999999999986, -460.39999999999986, -450.39999999999986, -440.39999999999986, -430.39999999999986, -420.39999999999986, -410.39999999999986, -400.39999999999986, -390.39999999999986, -380.39999999999986, -370.39999999999986, -360.39999999999986, -350.39999999999986, -340.39999999999986, -330.39999999999986, -320.39999999999986, -310.39999999999986, -300.39999999999986, -290.39999999999986, -280.39999999999986, -270.39999999999986, -260.39999999999986, -250.39999999999986, -240.39999999999986, -230.39999999999986, -220.39999999999986, -210.39999999999986, -200.39999999999986, -190.39999999999986, -180.39999999999986, -170.39999999999986, -160.39999999999986, -150.39999999999986, -140.39999999999986, -130.39999999999986, -120.39999999999986, -110.39999999999986, -100.39999999999986, -90.40000000000009, -80.40000000000009, -70.40000000000009, -60.40000000000009, -50.40000000000009, -40.40000000000009, -30.40000000000009, -20.40000000000009, -10.400000000000091, -0.40000000000009095, 9.599999999999909, 19.59999999999991, 29.59999999999991, 39.59999999999991, 49.59999999999991, 59.59999999999991, 69.59999999999991, 79.59999999999991, 89.59999999999991, 99.59999999999991, 109.59999999999991, 119.59999999999991, 129.5999999999999, 139.5999999999999, 149.5999999999999, 159.5999999999999, 169.5999999999999, 179.5999999999999, 189.5999999999999, 199.5999999999999, 209.5999999999999, 219.5999999999999, 229.5999999999999, 239.5999999999999, 249.5999999999999, 259.5999999999999, 269.5999999999999, 279.5999999999999, 289.5999999999999, 299.5999999999999, 309.5999999999999, 319.5999999999999, 329.5999999999999, 339.5999999999999, 349.5999999999999, 359.5999999999999, 369.5999999999999, 379.5999999999999, 389.5999999999999, 399.5999999999999, 409.5999999999999, 419.5999999999999, 429.5999999999999, 439.5999999999999, 449.5999999999999, 459.5999999999999, 469.5999999999999, 479.5999999999999, 489.5999999999999, 499.5999999999999, 509.5999999999999, 519.5999999999999, 529.5999999999999, 539.5999999999999, 549.5999999999999, 559.5999999999999, 569.5999999999999, 579.5999999999999, 589.5999999999999, 599.5999999999999, 609.5999999999999, 619.5999999999999, 629.5999999999999, 639.5999999999999, 649.5999999999999, 659.5999999999999, 669.5999999999999, 679.5999999999999, 689.5999999999999, 699.5999999999999, 709.5999999999999, 719.5999999999999, 729.5999999999999, 739.5999999999999 };
        public static double[,] zCorrections = new double[,] { { 41.9408, 41.1881, 40.4354, 39.6827, 38.93, 38.2268, 37.6128, 36.8355, 35.8344, 34.466, 33.0405, 31.3878, 30.0637, 28.2944, 26.8898, 25.2765, 23.713, 22.3985, 21.3003, 20.4896, 19.9967, 19.6688, 19.2481, 18.4421, 17.1001, 15.172, 12.8016, 10.4746, 8.57598, 7.2873, 6.2278, 5.34613, 4.57839, 3.80553, 3.00935, 2.30872, 1.46371, 0.202222, -1.09529, -2.56195, -3.94273, -5.01787, -5.45367, -5.35713, -5.26604, -5.31082, -5.21272, -4.91381, -4.49596, -4.145, -3.85529, -3.702, -3.79551, -4.07825, -4.65587, -5.21536, -5.58546, -5.53214, -4.95991, -3.8815, -2.79857, -1.87463, -1.30462, -1.10333, -1.32155, -1.83774, -2.68429, -3.77562, -4.70118, -5.09424, -4.96804, -4.92051, -5.19947, -5.80333, -6.25969, -6.39222, -6.32, -6.15476, -5.82146, -5.4075, -5.15431, -5.118, -5.017, -4.919, -5.04857, -5.24813, -5.73129, -6.79022, -8.25588, -10.0605, -11.9569, -13.3334, -13.765, -13.5594, -13.0498, -12.2163, -11.4076, -10.6107, -9.92094, -9.37189, -8.84143, -8.35667, -7.88429, -7.47485, -7.12673, -6.76423, -6.35654, -5.85491, -5.33226, -4.72189, -4.1681, -3.71095, -3.35077, -3.02825, -2.78845, -2.54353, -2.30592, -2.09157, -1.7749, -1.48827, -1.16126, -0.914118, -0.706634, -0.682121, -0.775051 }, { 39.4151, 38.7768, 38.1385, 37.5002, 36.8619, 36.3568, 35.8257, 35.0641, 34.1354, 33.0622, 31.9289, 30.4679, 29.03, 27.9135, 26.7289, 25.3982, 24.0278, 22.7567, 21.6113, 20.7896, 20.2362, 19.8867, 19.6206, 19.1417, 18.3266, 16.8182, 14.9002, 12.9181, 11.1783, 9.88111, 8.85857, 8.02783, 7.2214, 6.49129, 5.80979, 5.07894, 4.3429, 3.49484, 2.47556, 1.25472, 0.0411111, -1.07659, -1.70469, -1.71118, -1.52588, -1.34525, -1.28176, -1.12941, -0.933725, -0.748431, -0.532745, -0.393232, -0.458776, -0.724167, -1.19632, -1.77632, -2.26062, -2.415, -2.21762, -1.46364, -0.573636, 0.252222, 0.884286, 1.20129, 1.13825, 0.75, 0.0988172, -0.820549, -1.74204, -2.27206, -2.355, -2.28485, -2.41898, -2.82521, -3.11742, -3.28515, -3.325, -3.18863, -3.00689, -2.65725, -2.55283, -2.67097, -2.60861, -2.521, -2.549, -2.60535, -2.88263, -3.61457, -4.7631, -6.37814, -8.03047, -9.39667, -10.1822, -10.1846, -9.73604, -9.11206, -8.45411, -7.82327, -7.2681, -6.77952, -6.33381, -5.90208, -5.50689, -5.16709, -4.87777, -4.57, -4.27583, -3.92385, -3.51762, -3.09308, -2.70558, -2.37971, -2.08471, -1.80107, -1.58569, -1.33893, -1.152, -1.00204, -0.79549, -0.572941, -0.340588, -0.214118, -0.074, -0.0568687, -0.236327 }, { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 }, { -18.4081, -17.9186, -17.4583, -17.0181, -16.2924, -15.6756, -14.9434, -14.3161, -13.7461, -13.0643, -12.61, -12.0228, -11.3663, -10.7098, -9.95807, -9.1067, -8.28835, -7.49222, -6.73168, -6.10679, -5.61762, -5.21519, -4.91118, -4.63214, -4.29788, -3.88905, -3.44143, -2.95381, -2.50654, -2.12631, -1.79404, -1.43827, -1.08048, -0.695962, -0.298846, 0.077619, 0.396019, 0.712692, 1.03577, 1.34845, 1.69731, 2.13, 2.53385, 2.767, 2.808, 2.67184, 2.42388, 2.16673, 1.99364, 1.91485, 1.87545, 1.868, 1.91911, 2.06069, 2.15475, 2.37902, 2.60549, 2.78446, 2.87059, 2.88354, 2.67845, 2.31125, 1.90708, 1.58052, 1.37242, 1.289, 1.34683, 1.51932, 1.76689, 2.0999, 2.29961, 2.356, 2.334, 2.41812, 2.54881, 2.6003, 2.58, 2.50475, 2.47444, 2.2851, 2.05857, 1.96762, 2.05353, 2.20059, 2.36861, 2.495, 2.514, 2.51081, 2.62706, 2.99381, 3.57206, 4.17906, 4.7175, 5.12806, 5.40353, 5.5201, 5.505, 5.42, 5.45525, 5.38556, 5.32091, 5.30172, 5.17898, 5.00551, 4.85653, 4.64633, 4.41163, 4.18464, 3.90629, 3.61265, 3.31969, 3.10551, 2.89263, 2.75347, 2.60677, 2.42286, 2.25424, 2.08306, 1.94414, 1.75245, 1.60778, 1.544, 1.548, 1.50273, 1.64275 }, { -13.2455, -13.274, -13.5117, -13.9236, -14.1579, -13.9614, -13.3067, -12.5209, -11.525, -10.5132, -9.56091, -8.70303, -7.85257, -6.96091, -6.07982, -5.04364, -3.93957, -2.70304, -1.45421, -0.256607, 0.688182, 1.4083, 1.87757, 2.16529, 2.265, 2.35871, 2.51039, 2.86143, 3.31952, 3.77667, 4.17423, 4.55596, 5.04215, 5.65453, 6.15571, 6.70736, 7.23381, 7.65308, 7.90921, 8.15692, 8.53962, 9.01131, 9.73, 10.4052, 10.7179, 10.656, 10.4258, 9.87362, 9.26191, 8.72158, 8.25887, 7.96265, 7.76535, 7.7399, 7.73396, 7.98769, 8.37135, 8.75571, 9.09408, 9.286, 9.23103, 8.65258, 7.76077, 6.7637, 5.88161, 5.31969, 5.157, 5.31738, 5.67423, 6.20103, 6.90642, 7.29961, 7.346, 7.23101, 7.18204, 7.09327, 6.92286, 6.65887, 6.43722, 6.14546, 5.87082, 5.747, 5.94748, 6.08743, 6.34165, 6.506, 6.4698, 6.30879, 6.3399, 6.88963, 7.92912, 9.23174, 10.4344, 11.5229, 12.3179, 12.7271, 12.902, 12.8147, 12.6946, 12.5351, 12.3896, 12.181, 11.8998, 11.5258, 11.1058, 10.6584, 10.2196, 9.79842, 9.38833, 9.03309, 8.66229, 8.34031, 8.04735, 7.76958, 7.38104, 7.00938, 6.67639, 6.2925, 6.01041, 5.63632, 5.30938, 5.11163, 5.0, 5.03495, 5.02588 }, { -1.258, -1.22248, -1.3575, -2.08522, -2.93452, -3.5829, -3.945, -3.96109, -3.39569, -2.39411, -1.33518, -0.427407, 0.375872, 1.31468, 2.27909, 3.20838, 4.16364, 5.22469, 6.43261, 7.72913, 8.89726, 9.94364, 10.6262, 11.0624, 11.0967, 10.8912, 10.6657, 10.5583, 10.5508, 10.7082, 10.9251, 11.2251, 11.5875, 11.9685, 12.4538, 12.9658, 13.4867, 13.9586, 14.3319, 14.5241, 14.6202, 15.0396, 15.5504, 16.3606, 17.0052, 17.2488, 17.182, 16.8063, 16.1268, 15.2593, 14.4676, 13.7789, 13.3498, 13.0946, 13.0112, 13.1164, 13.4186, 13.7133, 13.9646, 14.2722, 14.3188, 13.9203, 12.9615, 11.6845, 10.4366, 9.51617, 9.043, 9.029, 9.28437, 9.69075, 10.3094, 10.8646, 11.0209, 10.8967, 10.808, 10.764, 10.5166, 10.3537, 10.0352, 9.53104, 9.00053, 8.54531, 8.523, 8.82151, 9.15692, 9.52029, 9.61586, 9.39907, 9.256, 9.6337, 10.5835, 11.9368, 13.3318, 14.6353, 15.674, 16.33, 16.6525, 16.737, 16.6896, 16.631, 16.5765, 16.4433, 16.2022, 15.7779, 15.3185, 14.7616, 14.1689, 13.564, 12.9563, 12.3605, 11.7826, 11.2605, 10.8144, 10.4217, 9.92588, 9.45632, 8.98417, 8.48876, 8.01, 7.56579, 7.11542, 6.84919, 6.70629, 6.67851, 6.657 } };
        public static double aziSin = -0.7071067811865475;
        public static double aziCos = -0.7071067811865477;
        public static double zCoordinateSpacing = 9.999999999999998;
        public static double firstZCoordinate = -500.39999999999986;
        public static double GetValue(double x, double y, double z)
        {
            double z_rescaled = (z - firstZCoordinate) / zCoordinateSpacing;
            int k = Math.Min(Math.Max((int)Math.Floor(z_rescaled), 0), (zCoordinates.Length) - 2);
            double fraction_z_above = z_rescaled - k;
            double fraction_z_below = k + 1 - z_rescaled;
            double nr = aziCos * x + aziSin * y;

            for (int j = 1; j < distancesFromOriginAlongTilt.Length; j++)
            {
                if ((nr < distancesFromOriginAlongTilt[j]) || (j == distancesFromOriginAlongTilt.Length - 1))
                {
                    double thisDistanceBinWidth = (distancesFromOriginAlongTilt[j] - distancesFromOriginAlongTilt[j - 1]);
                    double frac_at_lower = (distancesFromOriginAlongTilt[j] - nr) / thisDistanceBinWidth;
                    double frac_at_upper = (nr - distancesFromOriginAlongTilt[j - 1]) / thisDistanceBinWidth;
                    double val_at_lower = (zCorrections[j - 1, k + 1] * fraction_z_above + zCorrections[j - 1, k] * fraction_z_below);
                    double val_at_upper = (zCorrections[j, k + 1] * fraction_z_above + zCorrections[j, k] * fraction_z_below);
                    return (val_at_upper * frac_at_upper + val_at_lower * frac_at_lower);
                }
            }
            return 0;
        }
    }


    public static double anizoSin = 0.766044443118978;
    public static double anizoCos = -0.6427876096865394;
    public static double anK1 = 0.933326680078202;
    public static double anK2 = 1.0351020283761143;
    public static double anK3 = 1.0351020283761143;
    public static Vector3 AnizoAdjust(Vector3 dir)
    {
        Vector3 rv = new Vector3((float)anizoCos * dir.x + (float)anizoSin * dir.y, -(float)anizoSin * dir.x + (float)anizoCos * dir.y,dir.z);
        Vector3 adj1 = new Vector3(rv.x * (float)anK1, rv.y * (float)anK2, rv.z * (float)anK3);
        return adj1;
    }
    static public double getAnizoAbsScaling(double x, double y, double z) //direction
    {
        double l1 = anK1 * anK1;
        double l2 = anK2 * anK2;
        double l3 = anK3 * anK3;
        double B2 = 1.0 / l1 + 1.0 / l2 + 1.0 / l3;
        double n1 = anizoCos * x + anizoSin * y;
        double n2 = -anizoSin * x + anizoCos * y;
        double n3 = z;
        double s1 = n1 * n1;
        double s2 = n2 * n2;
        double s3 = n3 * n3;
        double nB = s1 / l1 + s2 / l2 + s3 / l3;
        double An = s1 * l1 + s2 * l2 + s3 * l3;
        double nr = (B2 - nB) * An / 2.0;
        return 1.0 / nr; // the absorption length is multiplied with this factor in the kernel
    }



    static double[] A = new double[] { 999.0, 0.0341635, 0.0413, 0.0330928, 0.0271464, 0.023892, 0.0143366, 0.00567721, 0.00264098, 0.00204705, 0.00257984, 0.00308348, 0.0029217, 0.00275415, 0.00257335, 0.00221773, 0.00238596, 0.00285785, 0.00357621, 0.00455018, 0.00553308, 0.0078887, 0.00572507, 0.00387496, 0.00592644, 0.0081376, 0.00470202, 0.0054081, 0.00550049, 0.00566188, 0.0039511, 0.00333618, 0.00321569, 0.00391968, 0.00266841, 0.00356561, 0.0039609, 0.00372861, 0.00429247, 0.00410074, 0.00469417, 0.00683495, 0.00588672, 0.0096173, 0.0128103, 0.0110762, 0.00464663, 0.00415293, 0.0043853, 0.00315576, 0.00280608, 0.00347515, 0.00344068, 0.0046345, 0.00461737, 0.00498439, 0.00517225, 0.00670364, 0.00634612, 0.00869914, 0.0100843, 0.00809331, 0.0111009, 0.00829113, 0.00620578, 0.00409576, 0.00665404, 0.00619234, 0.00668283, 0.00496042, 0.00621274, 0.0118167, 0.0228665, 0.0373826, 0.041686, 0.0287134, 0.0405051, 0.0326619, 0.0203612, 0.019504, 0.039754, 0.0220969, 0.0163531, 0.0104275, 0.013178, 0.0078097, 0.00647819, 0.00516209, 0.00433188, 0.00769932, 0.00547507, 0.00668387, 0.0111196, 0.0159089, 0.00971674, 0.0100242, 0.00916085, 0.00684982, 0.00588244, 0.00433883, 0.00613889, 0.0074258, 0.00776451, 0.0106442, 0.00777571, 0.010518, 0.0194443, 0.015566, 0.0140449, 0.0109999, 0.00585253, 0.00624764, 0.00559704, 0.00493057, 0.00641338, 0.00802777, 0.00870217, 0.0172207, 0.01209, 0.010891, 0.0116745, 0.013377, 0.0163123, 0.0143573, 0.0151353, 0.0238926, 0.0174822, 0.0148848, 0.00994676, 0.00686835, 0.00721764, 0.00627007, 0.00858937, 0.011207, 0.0129233, 0.0148122, 0.0169797, 0.0101364, 0.0202091, 0.0231987, 0.0227844, 0.0197689, 0.0169668, 0.02528, 0.0146585, 0.0255938, 0.0306032, 0.0316501, 0.0374324, 0.029917, 0.0244345, 0.0265984, 0.0281518, 0.0300968, 0.0357103, 0.0428928, 0.0453458, 0.0493356, 0.0442779, 0.0369883, 0.0361874, 0.0302938, 0.0295018, 0.0348879, 0.0470742, 0.0491141, 0.0446113, 0.0412168, 0.0429327, 0.0428217, 0.0422557 };
    static double[] B = new double[] { 1.1969400000000001, 1.2689300000000003, 1.5214900000000005, 1.2308500000000002, 1.01835, 0.9012240000000002, 0.5528440000000001, 0.22785500000000006, 0.10955600000000001, 0.08585740000000001, 0.10712800000000001, 0.12706000000000003, 0.12067400000000003, 0.11404400000000003, 0.10687000000000002, 0.09269470000000002, 0.09941200000000003, 0.11814900000000002, 0.14642300000000003, 0.18437400000000004, 0.22231700000000004, 0.3121480000000001, 0.19026100000000004, 0.18092600000000003, 0.23241500000000004, 0.3092230000000001, 0.22679900000000006, 0.22228700000000007, 0.19682900000000003, 0.21088300000000004, 0.17990500000000004, 0.16773000000000005, 0.11300900000000001, 0.14144100000000004, 0.09973400000000003, 0.14058400000000004, 0.12786700000000004, 0.12522300000000003, 0.17784700000000003, 0.18057100000000004, 0.22776600000000005, 0.22600800000000007, 0.2985830000000001, 0.28622500000000006, 0.38437100000000013, 0.2933490000000001, 0.18804500000000002, 0.17747100000000002, 0.15822900000000004, 0.12018700000000003, 0.13346700000000003, 0.13449300000000003, 0.15382400000000002, 0.13946900000000004, 0.18256700000000003, 0.19898400000000005, 0.18234200000000003, 0.21909000000000006, 0.20613900000000004, 0.35143100000000005, 0.34280200000000005, 0.31711300000000003, 0.36292500000000005, 0.309669, 0.18837100000000004, 0.17623800000000003, 0.23326100000000005, 0.3301520000000001, 0.2872970000000001, 0.18375200000000005, 0.20309900000000003, 0.4394870000000001, 0.8249210000000001, 1.1180400000000001, 1.6227800000000003, 1.5594900000000005, 1.3795300000000001, 1.1547500000000002, 0.8170120000000002, 0.9714880000000001, 1.2557500000000001, 1.0352200000000003, 0.4562970000000001, 0.5178720000000001, 0.4351250000000001, 0.3194770000000001, 0.3048100000000001, 0.21828400000000006, 0.21999900000000006, 0.28487300000000004, 0.24802800000000005, 0.25551900000000005, 0.47110900000000006, 0.3760820000000001, 0.5291910000000001, 0.3838690000000001, 0.30815500000000007, 0.27171200000000006, 0.23289900000000005, 0.21156800000000003, 0.3139640000000001, 0.2659540000000001, 0.2911870000000001, 0.3681870000000001, 0.4332370000000001, 0.49198900000000007, 0.4519460000000001, 0.4821530000000001, 0.46678500000000006, 0.36282100000000006, 0.38418900000000006, 0.23666400000000007, 0.2742530000000001, 0.21962600000000004, 0.27220200000000006, 0.25006700000000004, 0.4050630000000001, 0.5979920000000001, 0.4768500000000001, 0.4304300000000001, 0.29712800000000006, 0.6550780000000002, 0.6300690000000002, 0.6465330000000001, 0.4688530000000001, 0.7721230000000001, 0.6530230000000001, 0.5507960000000001, 0.29323400000000005, 0.2851950000000001, 0.28936800000000007, 0.2859770000000001, 0.3720480000000001, 0.5405010000000001, 0.5209400000000002, 0.5644640000000002, 0.5453550000000001, 0.43253600000000014, 0.6029750000000001, 0.7297120000000001, 0.6057890000000001, 0.6567140000000002, 0.9631470000000002, 0.8187000000000002, 0.6067460000000001, 1.1543800000000002, 1.1421000000000003, 1.1794600000000002, 1.3848700000000003, 1.1175900000000003, 0.9207950000000001, 0.9986720000000002, 1.05441, 1.1240100000000002, 1.3238500000000004, 1.5775900000000005, 1.6638100000000005, 1.8036200000000004, 1.6263000000000003, 1.3691500000000003, 1.3407700000000002, 1.1310500000000003, 1.1027400000000003, 1.2946600000000004, 1.7244400000000002, 1.7958700000000003, 1.6380100000000004, 1.5185500000000003, 1.5789900000000006, 1.5750900000000005, 1.5551600000000003 };
    static double[] deltaTau = new double[] { 27.8202, 27.5114, 27.2026, 26.8938, 26.585, 26.2762, 25.9674, 25.6586, 25.3498, 25.041, 24.7322, 24.4234, 24.1146, 23.8071, 23.5007, 23.1956, 22.8915, 22.5887, 22.2869, 21.9864, 21.687, 21.3888, 21.0918, 20.7959, 20.5012, 20.2076, 19.9152, 19.624, 19.3339, 19.0451, 18.7573, 18.4708, 18.1854, 17.9011, 17.618, 17.3361, 17.0554, 16.7758, 16.4974, 16.2201, 15.944, 15.6691, 15.3953, 15.1228, 14.8513, 14.5811, 14.312, 14.044, 13.7772, 13.5116, 13.2472, 12.9839, 12.7218, 12.4608, 12.2011, 11.9424, 11.685, 11.4287, 11.1736, 10.9196, 10.6668, 10.4151, 10.1647, 9.91538, 9.66722, 9.42024, 9.17444, 8.92978, 8.6863, 8.44399, 8.20282, 7.96283, 7.724, 7.48634, 7.24985, 7.01451, 6.78034, 6.54734, 6.31549, 6.08481, 5.8553, 5.62696, 5.39977, 5.17375, 4.9489, 4.72521, 4.50267, 4.28131, 4.06111, 3.84209, 3.62422, 3.40752, 3.19197, 2.9776, 2.76439, 2.55234, 2.34146, 2.13174, 1.92319, 1.71581, 1.50957, 1.30452, 1.10062, 0.897888, 0.69632, 0.495911, 0.296677, 0.098602, -0.098297, -0.294052, -0.488632, -0.682053, -0.874298, -1.06538, -1.25531, -1.44406, -1.63165, -1.81808, -2.00336, -2.18745, -2.37039, -2.55217, -2.73277, -2.91222, -3.0905, -3.26761, -3.44356, -3.61835, -3.79196, -3.96442, -4.13571, -4.30585, -4.47481, -4.64261, -4.80924, -4.97472, -5.13902, -5.30216, -5.46414, -5.62495, -5.78459, -5.94309, -6.1004, -6.25656, -6.41154, -6.56537, -6.71803, -6.86954, -7.01987, -7.16904, -7.31705, -7.46388, -7.60956, -7.75407, -7.89742, -8.0396, -8.18062, -8.32047, -8.45915, -8.59668, -8.73305, -8.86824, -9.00227, -9.13515, -9.26685, -9.39738, -9.52675, -9.65497, -9.78201, -9.9079, -10.0326 };
    static double[] layerStart = new double[] { -855.3999999999999, -845.3999999999999, -835.3999999999999, -825.3999999999999, -815.3999999999999, -805.3999999999999, -795.3999999999999, -785.3999999999999, -775.3999999999999, -765.3999999999999, -755.3999999999999, -745.3999999999999, -735.3999999999999, -725.3999999999999, -715.3999999999999, -705.3999999999999, -695.3999999999999, -685.3999999999999, -675.3999999999999, -665.3999999999999, -655.3999999999999, -645.3999999999999, -635.3999999999999, -625.3999999999999, -615.3999999999999, -605.3999999999999, -595.3999999999999, -585.3999999999999, -575.3999999999999, -565.3999999999999, -555.3999999999999, -545.3999999999999, -535.3999999999999, -525.3999999999999, -515.3999999999999, -505.39999999999986, -495.39999999999986, -485.39999999999986, -475.39999999999986, -465.39999999999986, -455.39999999999986, -445.39999999999986, -435.39999999999986, -425.39999999999986, -415.39999999999986, -405.39999999999986, -395.39999999999986, -385.39999999999986, -375.39999999999986, -365.39999999999986, -355.39999999999986, -345.39999999999986, -335.39999999999986, -325.39999999999986, -315.39999999999986, -305.39999999999986, -295.39999999999986, -285.39999999999986, -275.39999999999986, -265.39999999999986, -255.39999999999986, -245.39999999999986, -235.39999999999986, -225.39999999999986, -215.39999999999986, -205.39999999999986, -195.39999999999986, -185.39999999999986, -175.39999999999986, -165.39999999999986, -155.39999999999986, -145.39999999999986, -135.39999999999986, -125.39999999999986, -115.39999999999986, -105.39999999999986, -95.40000000000009, -85.40000000000009, -75.40000000000009, -65.40000000000009, -55.40000000000009, -45.40000000000009, -35.40000000000009, -25.40000000000009, -15.400000000000091, -5.400000000000091, 4.599999999999909, 14.599999999999909, 24.59999999999991, 34.59999999999991, 44.59999999999991, 54.59999999999991, 64.59999999999991, 74.59999999999991, 84.59999999999991, 94.59999999999991, 104.59999999999991, 114.59999999999991, 124.59999999999991, 134.5999999999999, 144.5999999999999, 154.5999999999999, 164.5999999999999, 174.5999999999999, 184.5999999999999, 194.5999999999999, 204.5999999999999, 214.5999999999999, 224.5999999999999, 234.5999999999999, 244.5999999999999, 254.5999999999999, 264.5999999999999, 274.5999999999999, 284.5999999999999, 294.5999999999999, 304.5999999999999, 314.5999999999999, 324.5999999999999, 334.5999999999999, 344.5999999999999, 354.5999999999999, 364.5999999999999, 374.5999999999999, 384.5999999999999, 394.5999999999999, 404.5999999999999, 414.5999999999999, 424.5999999999999, 434.5999999999999, 444.5999999999999, 454.5999999999999, 464.5999999999999, 474.5999999999999, 484.5999999999999, 494.5999999999999, 504.5999999999999, 514.5999999999999, 524.5999999999999, 534.5999999999999, 544.5999999999999, 554.5999999999999, 564.5999999999999, 574.5999999999999, 584.5999999999999, 594.5999999999999, 604.5999999999999, 614.5999999999999, 624.5999999999999, 634.5999999999999, 644.5999999999999, 654.5999999999999, 664.5999999999999, 674.5999999999999, 684.5999999999999, 694.5999999999999, 704.5999999999999, 714.5999999999999, 724.5999999999999, 734.5999999999999, 744.5999999999999, 754.5999999999999, 764.5999999999999, 774.5999999999999, 784.5999999999999, 794.5999999999999, 804.5999999999999, 814.5999999999999, 824.5999999999999, 834.5999999999999, 844.5999999999999 };
    static double layerSpacing = 10.0;
    public static double alpha_scat = 0.898608505726;
    public static int GetLayer(double x, double y, double z)
    {
        double zshift = Tilt.GetValue(x, y, z);
        z += zshift;
        double l_offset = z - layerStart[0];
        if (l_offset < 0) return 0;
        int layer = (int)Math.Floor(l_offset / layerSpacing);
        if (layer >= layerStart.Length) return layerStart.Length - 1;
        return layer;
    }
    public static float AbsorbtionLength(Vector3 pos, Vector3 dir, double wlen = 400)
    {
        int layer = GetLayer((double)pos.x, (double)pos.y, (double)pos.z);
       
        double alen = GetAbsorbLen(wlen, layer);
        double anizo = getAnizoAbsScaling((double)dir.x, (double)dir.y, (double)dir.z);
        return (float)(alen * anizo);
    }
    public static float ScatterLength(Vector3 pos, double wlen = 400)
    {
        int layer = GetLayer((double)pos.x, (double)pos.y, (double)pos.z);
        double slen = GetScatterLen(wlen, layer);
        return (float)(slen);
    }


    public static double A_par = 6954.09033203125;
    public static double B_par = 6617.75439453125;
    public static double D_len = 662.0807154019338;
    public static double kappa = 1.08410680294;

    public static double GetAbsorbLen(double wlen, int layer) //in nanometers, I3CLSimFunctionAbsLenIceCube
    {

        return 1.0 / ((D_len * A[layer]) * Math.Pow(wlen, -kappa) + A_par * Math.Exp(-B_par / wlen) * (1.0 + 0.01 * deltaTau[layer]));
    }

    public static double GetScatterLen(double wlen, int layer) //in nm
    {
        return 1.0 / (B[layer] * Math.Pow(wlen / 400.0, -alpha_scat));
    }


}


public class CascadeData
{
    public Vector3 location;
    public float radius;
    public Vector3 directionPreference;
    public float directionBias;
    public int energy;//int? photon number? 
    public float timeOffset;
    public CascadeData(Vector3 dir, float bias,int en,float time)
    {
        location = Vector3.zero;
        directionPreference = dir.normalized;
        directionBias = bias;
        if (directionBias < 0) directionBias = 0;
        if (directionBias >1) directionBias = 1;
        energy = en;
        timeOffset = time;
    }
    public List<ScatterPhoton> Emit(float duration)
    {
        List<ScatterPhoton> ret = new List<ScatterPhoton>();
        for (int i= 0; i < energy;i++)
        {
            Vector3 emitPos = location + radius * TrailmeshMaker.onUnitSphere();
            Vector3 perp = new Vector3(-directionPreference.y, directionPreference.x, 0);
            if (directionPreference.z > 0.9)
            {
                perp = new Vector3(0, directionPreference.z, -directionPreference.y);
            }
            Quaternion rt = Quaternion.AngleAxis((float)TrailmeshMaker.rndgen.NextDouble() * 360, directionPreference);
            perp = (rt * perp).normalized;
            float angl = Mathf.PI * (1 - directionBias)* (float)TrailmeshMaker.rndgen.NextDouble();
            Vector3 dir = (directionPreference * Mathf.Cos(angl) + perp * Mathf.Sin(angl)).normalized;
            ScatterPhoton pht = new ScatterPhoton();
           // Debug.LogFormat("Derection:{0} {1}", dir, angl);
            pht.direction = dir;
            pht.position = emitPos;
            pht.timeOffset = timeOffset+duration* (float)TrailmeshMaker.rndgen.NextDouble();
            ret.Add(pht);
        }
        return ret;
    }
}
public class MuonTrack
{
    System.Random rndgen = new System.Random();
    public float duration;
    public float speed;
    public Vector3 initialPos;
    public Vector3 direction;
    public float emissionRate; //per nanosecond...
    public CascadeData[] energyLosses = null;
    public static float sin41 = Mathf.Sin(41.0f * Mathf.PI / 180.0f);
    public static float cos41 = Mathf.Cos(41.0f * Mathf.PI / 180.0f);
    ScatterPhoton EmitCherenkovPhoton(Vector3 pos,float timeOffset)
    {
        float shift = (float)rndgen.NextDouble();
        Vector3 perp = new Vector3(-direction.y, direction.x, 0);
        if (Mathf.Abs(direction.z)>0.9)
        {
            perp= new Vector3(0, direction.z, -direction.y);
        }
        Quaternion rt = Quaternion.AngleAxis(shift * 360, direction);
        perp = rt * perp;
        perp /= perp.magnitude;
        Vector3 photondir = direction * cos41 + perp * sin41;
        ScatterPhoton ret = new ScatterPhoton();
        ret.direction = photondir;
        ret.position = pos;
        ret.timeOffset = timeOffset;
        //ret.Propagate(IcecubeDust.getAbsorption(pos), IcecubeDust.getScatter(pos));
        return ret;
    }
    public MuonTrack(Vector3 start,Vector3 orientation, float dur,float speed,float emrate,params CascadeData[] cascades)
    {
        initialPos = start;
        direction = orientation/orientation.magnitude;
        duration = dur;
        this.speed = speed;
        emissionRate = emrate;
        energyLosses = cascades;
    }
    public MultimeshObject PhotonsToMesh(List<ScatterPhoton> photons)
    {
        MultimeshObject ret = new MultimeshObject();
        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        //List<Color> clrs = new List<Color>();
        List<int> indc = new List<int>();
        foreach(ScatterPhoton ph in photons)
        {
            int cnt = 0;
            while(!ph.decayed)
            {
                verts.Add(ph.position-initialPos);
               // float shft = (en.time - mintime) / td;
                uvs.Add(new Vector2(ph.timeOffset, 0));
                //65530
                // clrs.Add(Color.Lerp(Color.red, Color.green, shft));   
                if(cnt==0 && ph.decayed)
                {
                    verts.RemoveAt(verts.Count-1);
                    uvs.RemoveAt(uvs.Count - 1);
                }
                if(cnt>0)
                {
                    indc.Add(verts.Count - 2);
                    indc.Add(verts.Count - 1);
                }
                if (verts.Count > 65530) break;
               // Debug.LogFormat("Phdir: {0}", ph.direction);
                ph.Propagate(IcecubeDust.AbsorbtionLength(ph.position,ph.direction), IcecubeDust.ScatterLength(ph.position));
                cnt += 1;
            }
            if(verts.Count > 65530)
            {
                Meshobject newM = new Meshobject(verts.ToArray(),uvs.ToArray(),indc.ToArray());
                ret.meshes.Add(newM);
                verts.Clear();
                uvs.Clear();
                indc.Clear();
                   
            }
        }
        if (indc.Count>1)
        {
            Meshobject newM = new Meshobject(verts.ToArray(), uvs.ToArray(), indc.ToArray());
            ret.meshes.Add(newM);
        }
        return ret;
    }
    virtual public MultimeshObject Simulate()
    {
        int emittedPhotons = Mathf.RoundToInt(emissionRate * duration);
        MultimeshObject  ret = new MultimeshObject();
        List<ScatterPhoton> emitted = new List<ScatterPhoton>();
        for(int i=0;i<emittedPhotons;i++)
        {
            float emitTime = (float)rndgen.NextDouble() * duration;
            Vector3 ppos = emitTime * speed * direction + initialPos;
            emitted.Add(EmitCherenkovPhoton(ppos,emitTime));
            if (emitted.Count>= 16384)
            {
                ret.meshes.AddRange(PhotonsToMesh(emitted).meshes);
                emitted.Clear();
            }
        }
        ret.meshes.AddRange(PhotonsToMesh(emitted).meshes);
        if (energyLosses != null)
        {
            foreach (CascadeData dat in energyLosses)
            {
                emitted = dat.Emit( 0.02f);
                ret.meshes.AddRange(PhotonsToMesh(emitted).meshes);
            }
         }
        return ret;
    }
}
public class TrailmeshMaker : MonoBehaviour
{
    // Start is called before the first frame update
    static Quaternion rot = Quaternion.Euler(new Vector3(0,0,125));
    static float al = Mathf.Exp(-0.082f);
    static float bet = Mathf.Exp(0.040f);
    static float gm = Mathf.Exp(0.042f);
    static public  System.Random rndgen = new System.Random();
    void Start()
    {
        /*trackData frc = new trackData();
         frc.azi_rad = 2.160849318134296f;
         frc.dec_rad = 0.18339852545918298f;
         frc.mjd = 58694.86853369251;
         frc.ra_rad = 3.958934679276621;
         frc.rec_t0 = 17437.752211638854f;
         frc.rec_x = -175.9504622785223f;
         frc.rec_y = -76.50362830103359f;
         frc.rec_z = -502.3460613431437f;
         frc.zen_rad = 1.752881090566949f;
         MakeEventTrail(frc); */
        //MakeTrail();  
        float acc = 100;
        for(int i=0;i<1000;i++)
        {
            acc += DrawExp(100);
        }
        Debug.LogFormat("Average: {0}", acc / 1000.0f);
    }
    public static float DrawExp(float ilam)
    {
        if (ilam <= 0) ilam = 0.01f;
        float ran = (float)rndgen.NextDouble() ;
       
        float ret = -Mathf.Log(1.0f - ran) *ilam ;
        if (ret > ilam * 10000) return ilam * 10000;
        return ret;
    }
    public static float scatterprob(float cosn)
    {
        float g = 0.94f;
        float alpha = 2 * g / (1 - g);
        float liu = 0.5f * (1 + alpha) * Mathf.Pow(0.5f*(1+cosn), alpha);
        float hg = 0.5f * (1 - g * g) /Mathf.Pow (1+g*g-2*g*cosn,1.5f);
        return 0.55f * hg + 0.45f * liu;
    }
    public static float scatterdirprob(Vector3 dir, Vector3 d2)
    {

        Vector3 adj1 = IcecubeDust.AnizoAdjust(dir);
        Vector3 adj2 = IcecubeDust.AnizoAdjust(d2);
       float cosn = Vector3.Dot(adj1, adj2) / (adj1.magnitude * adj2.magnitude);
       return scatterprob(cosn);
    }
    static public Vector3 onUnitSphere()
    {
        while (true)
        {
            Vector3 vct = new Vector3((float)(rndgen.NextDouble() * 2 - 1), (float)(rndgen.NextDouble() * 2 - 1), (float)(rndgen.NextDouble() * 2 - 1));
            var mag = vct.magnitude;
            if (mag <= 1 && mag > 0.0001f)
            {
            
                return vct / mag;
            }
        }
    }
    // Start is called before the first frame update
    
    public static Vector3 Scatter(Vector3 inc)
    {
        int cnt = 0;
        while(true)
        {
            Vector3 rd = onUnitSphere();
            float prob = scatterdirprob(inc, rd);
           
            if (prob>= rndgen.NextDouble()*40)
            {
                //Debug.LogFormat("Prob: {0}",prob);
                return rd;
            }
            if (cnt > 100) return inc;
            cnt += 1;
        }
    }
    public void MakeTrail()
    {
       
        
      /*  MuonTrack track = new MuonTrack(new Vector3(0, 0, 0), Random.onUnitSphere, 500, 2.99792458f/1.001f, 100);
        MultimeshObject meshes = track.Simulate();
        int cnt = 0;
        foreach(Meshobject m in meshes.meshes)
        {
            AssetDatabase.CreateAsset(m.ToMesh(), "Assets/Trailmesh/tmesh2_" + cnt.ToString() + ".asset");
            cnt += 1;
        }*/
        
      
        //            }

    }
    public static void MakeEventTrail(trackData track)
    {
       /* Vector3 tdir = new Vector3(Mathf.Sin(track.zen_rad) * Mathf.Cos(track.azi_rad), Mathf.Sin(track.zen_rad) * Mathf.Sin(track.azi_rad), Mathf.Cos(track.zen_rad));
        Vector3 ipos = new Vector3(track.rec_x, track.rec_y, track.rec_z);
        MuonTrack mtrack = new MuonTrack(ipos, tdir,5000, 0.299792458f / 1.00000001f, 20);
        MultimeshObject meshes = mtrack.Simulate();
        int cnt = 0;
        foreach (Meshobject m in meshes.meshes)
        {
            AssetDatabase.CreateAsset(m.ToMesh(), "Assets/Trailmesh/tmesh2_" + cnt.ToString() + ".asset");
            cnt += 1;
        }*/
    }

}
