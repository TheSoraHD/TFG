using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using System;


public class Utils 
{
    // Debug Vector3
	public static void PrintVector3(string name, Vector3 v)
	{
		Debug.Log(name + " " + v.x + ", " + v.y + ", " + v.z + "\n");
	}

    // Safe string to float conversion
    public static float StringToFloat(string s)
    {
		return float.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
    }     

    // Extended sign: returns -1, 0 or 1 based on sign of a
    public static float sgn(float a)
    {
        if (a > 0.0f) return 1.0f;
        if (a < 0.0f) return -1.0f;
        return 0.0f;
    }

    // Gets the local IP
    public static string GetLocalIPAddress()
    {
        var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new System.Exception("No network adapters!");
    }

    // Find GameObject even if it is not active
    public static GameObject FindEvenIfNotActive(string name)
    {
        var objs = Resources.FindObjectsOfTypeAll<GameObject>();
        for (uint i = 0; i < objs.Length; ++i)
        {
            GameObject obj = objs[i];
            if (obj.name == name) return obj;
        }
        return null;
    }

    // Find transform 'name' in descendants of 'current'
    public static Transform FindDescendants(Transform current, string name)
    {
        if (current.name == name)
        {
            return current;
        }
        else
        {
            for (int i = 0; i < current.childCount; ++i)
            {
                Transform found = FindDescendants(current.GetChild(i), name);
                if (found != null)
                {
                    return found;
                }
            }
        }
        return null;
    }

    // Find any transform in 'names' in descendants of 'current'
    public static Transform FindDescendants(Transform current, string[] names)
    {
        foreach (string name in names)
        {
            if (Utils.FindDescendants(current, name))
            {
                return Utils.FindDescendants(current, name);
            }
        }
        return null;
    }

    // Record base stations transform
    public static void RecordBaseStations(GameObject baseStation1, GameObject baseStation2)
    {
        string line = System.DateTime.Now.ToString() + " ";
        if (baseStation1)
        {
            line += baseStation1.transform.position.x + " " + baseStation1.transform.position.y + " " + baseStation1.transform.position.z + " ";
            line += baseStation1.transform.eulerAngles.x + " " + baseStation1.transform.eulerAngles.y + " " + baseStation1.transform.eulerAngles.z + " ";
        }
        line += System.Environment.NewLine;
        line += System.DateTime.Now.ToString() + " ";
        if (baseStation2)
        {
            line += baseStation2.transform.position.x + " " + baseStation2.transform.position.y + " " + baseStation2.transform.position.z + " ";
            line += baseStation2.transform.eulerAngles.x + " " + baseStation2.transform.eulerAngles.y + " " + baseStation2.transform.eulerAngles.z + " ";
        }
        line += System.Environment.NewLine;
        System.IO.File.AppendAllText(".log_basestations.txt", line);
    }

	// Fit least square errors plane to set of points
	public static bool FitPlane(uint numPoints, Vector3[] points, ref float a, ref float b, ref float c, ref float d)
	{
		// Check input
		if (numPoints < 3)
		{
			return false;
		}

		// Compute the mean of the points
		Vector3 mean = new Vector3(0.0f, 0.0f, 0.0f);
		for (uint i = 0; i < numPoints; ++i)
		{
			mean += points[i];
		}
		mean /= numPoints;

		// Compute the linear system matrix and vector elements
		float xxSum = 0.0f, xySum = 0.0f, xzSum = 0.0f, yySum = 0.0f, yzSum = 0.0f;
		for (uint i = 0; i < numPoints ; ++i)
		{
			Vector3 diff = points[i] - mean;
			xxSum += diff[0] * diff[0];
			xySum += diff[0] * diff[1];
			xzSum += diff[0] * diff[2];
			yySum += diff[1] * diff[1];
			yzSum += diff[1] * diff[2];
		}

		// Solve the linear system
		float det = xxSum * yySum - xySum * xySum;
		if (det != 0.0f)
		{
			// Compute the fitted plane
			a = ( yySum * xzSum - xySum * yzSum ) / det;
			b = ( xxSum * yzSum - xySum * xzSum ) / det;
			c = -1;
			d = - a * mean[0] - b * mean[1] + mean[2]; 
			return true;
		}
		else
		{
			return false;
		}
	}

    // Fit least square errors sphere to set of points
    // http://www.janssenprecisionengineering.com/downloads/Fit-sphere-through-points.pdf
    public static bool FitSphere(uint numPoints, Vector3[] points, ref float radius, ref Vector3 center, ref float quality)
    {
        // Check input
        if (numPoints < 5)
        {
            return false;
        }

        // Compute the mean of the points
        Vector3 mean = new Vector3(0.0f, 0.0f, 0.0f);
        for (int i = 0; i < numPoints; ++i)
        {
            mean += points[i];
        }
        mean /= numPoints;

        // Compute A & B
        Vector3 ARow1 = new Vector3();
        Vector3 ARow2 = new Vector3();
        Vector3 ARow3 = new Vector3();
        Vector3 B = new Vector3();
        for (int i = 0; i < numPoints; ++i)
        {
            Vector3 point = points[i];
            Vector3 diff = point - mean;

            ARow1.x += point.x * diff.x;
            ARow1.y += point.x * diff.y;
            ARow1.z += point.x * diff.z;

            ARow2.x += point.y * diff.x;
            ARow2.y += point.y * diff.y;
            ARow2.z += point.y * diff.z;

            ARow3.x += point.z * diff.x;
            ARow3.y += point.z * diff.y;
            ARow3.z += point.z * diff.z;

            B.x += point.sqrMagnitude * diff.x;
            B.y += point.sqrMagnitude * diff.y;
            B.z += point.sqrMagnitude * diff.z;
        }

        ARow1 /= numPoints;
        ARow2 /= numPoints;
        ARow3 /= numPoints;
        B /= numPoints;

        ARow1 *= 2.0f;
        ARow2 *= 2.0f;
        ARow3 *= 2.0f;

        Matrix3x3 A = new Matrix3x3(new Vector3(ARow1.x, ARow2.x, ARow3.x),
                                    new Vector3(ARow1.y, ARow2.y, ARow3.y),
                                    new Vector3(ARow1.z, ARow2.z, ARow3.z));

        // Compute Center: c = (A^T * A)^-1 * A^T * B
        Matrix3x3 AT = A.transpose;
        Matrix3x3 inverseATA = (AT * A).inverse;
        center = inverseATA * AT * B;

        // Compute Radius
        float sumR = 0.0f;
        for (int i = 0; i < numPoints; ++i)
        {
            Vector3 point = points[i];
            float xDiff = point.x - center.x;
            float yDiff = point.y - center.y;
            float zDiff = point.z - center.z;
            sumR += xDiff * xDiff + yDiff * yDiff + zDiff * zDiff;
        }

        radius = Mathf.Sqrt(sumR / numPoints);

        // Compute Quality (a value close to 1 indicates a good fit quality)
        float max = 0.0f;
        quality = 0.0f;
        for (int i = 0; i < numPoints; ++i)
        {
            Vector3 point = points[i];
            float distance = (point - mean).sqrMagnitude;
            if (distance > max)
            {
                distance = max;
            }
            quality += Mathf.Abs((point - center).sqrMagnitude - (radius * radius));
        }

        return true;
    }

    public static Quaternion ExtractRotation(Matrix4x4 matrix)
    {
        Vector3 forward;
        forward.x = matrix.m02;
        forward.y = matrix.m12;
        forward.z = matrix.m22;
 
        Vector3 upwards;
        upwards.x = matrix.m01;
        upwards.y = matrix.m11;
        upwards.z = matrix.m21;
 
        return Quaternion.LookRotation(forward, upwards);
    }
 
    public static Vector3 ExtractPosition(Matrix4x4 matrix)
    {
        Vector3 position;
        position.x = matrix.m03;
        position.y = matrix.m13;
        position.z = matrix.m23;
        return position;
    }
 
    public static Vector3 ExtractScale(Matrix4x4 matrix)
    {
        Vector3 scale;
        scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
        scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
        scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
        return scale;
    }

	public static GameObject createPrimitiveSphere(Vector3 center, float radius, Color? color = null)
	{
		GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		sphere.transform.position = center;
		sphere.transform.localScale = new Vector3(radius*2.0f, radius*2.0f, radius*2.0f);
        sphere.tag = "DebugSphere";

        var renderer = sphere.GetComponent<MeshRenderer>();
        //keep old code intact, opened it for parametrizing a color
        renderer.material.SetColor("_Color", color.GetValueOrDefault(Color.white));

        return sphere;
	}

    public static Vector3 getMeanOfSamples(uint number_samples, Vector3[] samples)
    {
        Vector3 mean = new Vector3(0.0f, 0.0f, 0.0f);
        foreach (Vector3 sample in samples)
        mean+= sample;
        mean /= number_samples;

        Debug.Log("Distance first sample to mean : " + (samples[0] - mean).magnitude.ToString("F4"));
        return mean;
    }

    public static void exportToCSV(string folder, string filename, string[] nameColumns, List<string[]> rowValues)
    {
        List<string[]> rowData = new List<string[]>();
      
        if (nameColumns != null) rowData.Add(nameColumns);

        foreach (string[] element in rowValues)
        {
            rowData.Add(element);
        }

        string[][] output = new string[rowData.Count][];

        for(int i = 0; i < output.Length; i++)
        {
            output[i] = rowData[i];
        }

        int length = output.GetLength(0);
        string delimiter = ";";

        StringBuilder sb = new StringBuilder();

        for (int index = 0; index < length; index++)
        {
            sb.AppendLine(string.Join(delimiter, output[index]));
        }
        string folderPath = Path.Combine("data", folder);
        if (!System.IO.Directory.Exists(folderPath)) System.IO.Directory.CreateDirectory(folderPath);
        string filePath = Path.Combine(folderPath, filename + "__0.csv");
        int f = 1;
        while (System.IO.File.Exists(filePath))
        {
            filePath = Path.Combine(folderPath, filename + "__" + f++ + ".csv");
        }
        System.IO.File.WriteAllText(filePath, sb.ToString());
        Debug.Log("Saving Exercise in: " + filePath);
    }
	
	//Note that in 3d, two lines do not intersect most of the time. Use it if the Lines are in the same Plane.
    public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {
        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        //is coplanar, and not parallel
        if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
        {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + (lineVec1 * s);
            return true;
        }
        else
        {
            intersection = Vector3.zero;
            return false;
        }
    }
}
