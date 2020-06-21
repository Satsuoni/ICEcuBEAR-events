using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constraint
{
    public double[] variables;
    public double b;
    public int sign;

    public Constraint(double[] variables, double b,int sign)
    {
       
            this.variables = variables;
            this.b = b;
            this.sign = sign;

       
    }
}
public class SimplexIndexResult
{
    public Tuple<int, int> index;
    public SimplexResult result;

    public SimplexIndexResult(Tuple<int, int> index, SimplexResult result)
    {
        this.index = index;
        this.result = result;
    }
}
public class Function
{
    public double[] variables;
    public double c;
    public bool isExtrMax;

    public Function(double[] variables, double c, bool isExtrMax)
    {
        this.variables = variables;
        this.c = c;
        this.isExtrMax = isExtrMax;
    }
}

public class SimplexSnap
{
    public double[] b;
    public double[][] matrix;
    public double[] M;
    public double[] F;
    public int[] C;
    public double fValue;
    public double[] fVars;
    public bool isMDone;
    public bool[] m;

    public SimplexSnap(double[] b, double[][] matrix, double[] M, double[] F, int[] C, double[] fVars, bool isMDone, bool[] m)
    {
        this.b = Copy(b);
        this.matrix = Copy(matrix);
        this.M = Copy(M);
        this.F = Copy(F);
        this.C = Copy(C);
        this.isMDone = isMDone;
        this.m = Copy(m);
        this.fVars = Copy(fVars);
        fValue = 0;
        for (int i = 0; i < C.Length; i++)
        {
            fValue += fVars[C[i]] * b[i];
        }
    }

    T[] Copy<T>(T[] array)
    {
        T[] newArr = new T[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            newArr[i] = array[i];
        }
        return newArr;
    }

    T[][] Copy<T>(T[][] matrix)
    {
        T[][] newMatr = new T[matrix.Length][];
        for (int i = 0; i < matrix.Length; i++)
        {
            newMatr[i] = new T[matrix[0].Length];
            for (int j = 0; j < matrix[0].Length; j++)
            {
                newMatr[i][j] = matrix[i][j];
            }
        }
        return newMatr;
    }
}

public enum SimplexResult { Unbounded, Found, NotYetFound }

public class Simplex
{

    Function function;

    double[] functionVariables;
    double[][] matrix;
    double[] b;
    bool[] m;
    double[] M;
    double[] F;
    int[] C;
    bool isMDone = false;
    public Simplex(Function function, Constraint[] constraints)
    {
        if (function.isExtrMax)
        {
            this.function = function;
        }
        else
        {
            this.function = Canonize(function);
        }

        getMatrix(constraints);
        getFunctionArray();
        getMandF();

        for (int i = 0; i < F.Length; i++)
        {
            F[i] = -functionVariables[i];
        }

    }

    public Tuple<List<SimplexSnap>, SimplexResult> GetResult()
    {
        List<SimplexSnap> snaps = new List<SimplexSnap>();
        snaps.Add(new SimplexSnap(b, matrix, M, F, C, functionVariables, isMDone, m));

        SimplexIndexResult result = nextStep();
        int i = 0;
        while (result.result == SimplexResult.NotYetFound && i < 100)
        {
            calculateSimplexTableau(result.index);
            snaps.Add(new SimplexSnap(b, matrix, M, F, C, functionVariables, isMDone, m));
            result = nextStep();
            i++;
        }

        return new Tuple<List<SimplexSnap>, SimplexResult>(snaps, result.result);
    }

    void calculateSimplexTableau(Tuple<int, int> Xij)
    {
        double[][] newMatrix = new double[matrix.Length][];

        C[Xij.Item2] = Xij.Item1;

        double[] newJRow = new double[matrix.Length];

        for (int i = 0; i < matrix.Length; i++)
        {
            newJRow[i] = matrix[i][Xij.Item2] / matrix[Xij.Item1][Xij.Item2];
        }

        double[] newB = new double[b.Length];

        for (int i = 0; i < b.Length; i++)
        {
            if (i == Xij.Item2)
            {
                newB[i] = b[i] / matrix[Xij.Item1][Xij.Item2];
            }
            else
            {
                newB[i] = b[i] - b[Xij.Item2] / matrix[Xij.Item1][Xij.Item2] * matrix[Xij.Item1][i];
            }
        }

        b = newB;

        for (int i = 0; i < matrix.Length; i++)
        {
            newMatrix[i] = new double[C.Length];
            for (int j = 0; j < C.Length; j++)
            {
                if (j == Xij.Item2)
                {
                    newMatrix[i][j] = newJRow[i];
                }
                else
                {
                    newMatrix[i][j] = matrix[i][j] - newJRow[i] * matrix[Xij.Item1][j];
                }
            }
        }

        matrix = newMatrix;
        getMandF();
    }

    void getMandF()
    {
        M = new double[matrix.Length];
        F = new double[matrix.Length];

        for (int i = 0; i < matrix.Length; i++)
        {
            double sumF = 0;
            double sumM = 0;
            for (int j = 0; j < matrix[0].Length; j++)
            {
                if (m[C[j]])
                {
                    sumM -= matrix[i][j];
                }
                else
                {
                    sumF += functionVariables[C[j]] * matrix[i][j];
                }
            }
            M[i] = m[i] ? sumM + 1 : sumM;
            F[i] = sumF - functionVariables[i];
        }
    }

    SimplexIndexResult nextStep()
    {

        int columnM = getIndexOfNegativeElementWithMaxAbsoluteValue(M);

        if (isMDone || columnM == -1)
        {
            //M doesn't have negative values
            isMDone = true;
            int columnF = getIndexOfNegativeElementWithMaxAbsoluteValue(F);

            if (columnF != -1) //Has at least 1 negative value
            {
                int row = getIndexOfMinimalRatio(matrix[columnF], b);

                if (row != -1)
                {
                    return new SimplexIndexResult(new Tuple<int, int>(columnF, row), SimplexResult.NotYetFound);
                }
                else
                {
                    return new SimplexIndexResult(null, SimplexResult.Unbounded);
                }
            }
            else
            {
                return new SimplexIndexResult(null, SimplexResult.Found);
            }

        }
        else
        {
            int row = getIndexOfMinimalRatio(matrix[columnM], b);

            if (row != -1)
            {
                return new SimplexIndexResult(new Tuple<int, int>(columnM, row), SimplexResult.NotYetFound);
            }
            else
            {
                return new SimplexIndexResult(null, SimplexResult.Unbounded);
            }
        }
    }

    int getIndexOfNegativeElementWithMaxAbsoluteValue(double[] array)
    {
        int index = -1;

        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] < 0)
            {
                if (!isMDone || isMDone && !m[i])
                {
                    if (index == -1)
                    {
                        index = i;
                    }
                    else if (Math.Abs(array[i]) > Math.Abs(array[index]))
                    {
                        index = i;
                    }
                }

            }
        }
        return index;
    }

    int getIndexOfMinimalRatio(double[] column, double[] b)
    {
        int index = -1;

        for (int i = 0; i < column.Length; i++)
        {
            if (column[i] > 0 && b[i] > 0)
            {
                if (index == -1)
                {
                    index = i;
                }
                else if (b[i] / column[i] < b[index] / column[index])
                {
                    index = i;
                }
            }
        }

        return index;
    }

    public void getFunctionArray()
    {
        double[] funcVars = new double[matrix.Length];
        for (int i = 0; i < matrix.Length; i++)
        {
            funcVars[i] = i < function.variables.Length ? function.variables[i] : 0;
        }
        this.functionVariables = funcVars;
    }

    public Function Canonize(Function function)
    {
        double[] newFuncVars = new double[function.variables.Length];

        for (int i = 0; i < function.variables.Length; i++)
        {
            newFuncVars[i] = -function.variables[i];
        }
        return new Function(newFuncVars, -function.c, true);
    }

    double[][] appendColumn(double[][] matrix, double[] column)
    {
        double[][] newMatrix = new double[matrix.Length + 1][];
        for (int i = 0; i < matrix.Length; i++)
        {
            newMatrix[i] = matrix[i];
        }
        newMatrix[matrix.Length] = column;
        return newMatrix;
    }

    T[] append<T>(T[] array, T element)
    {
        T[] newArray = new T[array.Length + 1];
        for (int i = 0; i < array.Length; i++)
        {
            newArray[i] = array[i];
        }
        newArray[array.Length] = element;
        return newArray;
    }

    double[] getColumn(double value, int place, int length)
    {
        double[] newColumn = new double[length];

        for (int k = 0; k < length; k++)
        {
            newColumn[k] = k == place ? value : 0;
        }

        return newColumn;
    }

    public void getMatrix(Constraint[] constraints)
    {
        for (int i = 0; i < constraints.Length; i++)
        {
            if (constraints[i].b < 0)
            {
                double[] cVars = new double[constraints[i].variables.Length];

                for (int j = 0; j < constraints[i].variables.Length; j++)
                {
                    cVars[j] = -constraints[i].variables[j];
                }

                int sign = -constraints[i].sign;


                Constraint cNew = new Constraint(cVars, -constraints[i].b, sign);
                constraints[i] = cNew;
            }
        }

        double[][] matrix = new double[constraints[0].variables.Length][];

        for (int i = 0; i < constraints[0].variables.Length; i++)
        {
            matrix[i] = new double[constraints.Length];
            for (int j = 0; j < constraints.Length; j++)
            {
                matrix[i][j] = constraints[j].variables[i];
            }
        }

        double[][] appendixMatrix = new double[0][];
        double[] Bs = new double[constraints.Length];

        for (int i = 0; i < constraints.Length; i++)
        {
            Constraint current = constraints[i];

            Bs[i] = current.b;

            if (current.sign > 0)
            {
                appendixMatrix = appendColumn(appendixMatrix, getColumn(-1, i, constraints.Length));
            }
            else if (current.sign < 0)
            {
                appendixMatrix = appendColumn(appendixMatrix, getColumn(1, i, constraints.Length));
            }
        }

        double[][] newMatrix = new double[constraints[0].variables.Length + appendixMatrix.Length][];

        for (int i = 0; i < constraints[0].variables.Length; i++)
        {
            newMatrix[i] = matrix[i];
        }

        for (int i = constraints[0].variables.Length; i < constraints[0].variables.Length + appendixMatrix.Length; i++)
        {
            newMatrix[i] = appendixMatrix[i - constraints[0].variables.Length];
        }

        bool[] hasBasicVar = new bool[constraints.Length];

        for (int i = 0; i < constraints.Length; i++)
        {
            hasBasicVar[i] = false;
        }

        C = new int[constraints.Length];

        int ci = 0;
        for (int i = 0; i < newMatrix.Length; i++)
        {


            bool hasOnlyNulls = true;
            bool hasOne = false;
            Tuple<int, int> onePosition = new Tuple<int, int>(0, 0);
            for (int j = 0; j < constraints.Length; j++)
            {


                if (newMatrix[i][j] == 1)
                {
                    if (hasOne)
                    {
                        hasOnlyNulls = false;
                        break;
                    }
                    else
                    {
                        hasOne = true;
                        onePosition = new Tuple<int, int>(i, j);
                    }
                }
                else if (newMatrix[i][j] != 0)
                {
                    hasOnlyNulls = false;
                    break;
                }


            }

            if (hasOnlyNulls && hasOne)
            {
                hasBasicVar[onePosition.Item2] = true;
                C[ci] = onePosition.Item1;
                ci++;
            }

        }

        m = new bool[newMatrix.Length];

        for (int i = 0; i < newMatrix.Length; i++)
        {
            m[i] = false;
        }

        for (int i = 0; i < constraints.Length; i++)
        {

            if (!hasBasicVar[i])
            {

                double[] basicColumn = new double[constraints.Length];

                for (int j = 0; j < constraints.Length; j++)
                {
                    basicColumn[j] = j == i ? 1 : 0;
                }

                newMatrix = appendColumn(newMatrix, basicColumn);
                m = append(m, true);
                C[ci] = newMatrix.Length - 1;
                ci++;
            }

        }

        this.b = Bs;
        this.matrix = newMatrix;
    }
}
public enum rectSide
{
    Top,
    Bottom,
    Left,
    Right,
    CenterX,
    CenterY,
    Width,
    Height
}

public enum Relation
{
    LessThan =-1,
    Equal=0,
    GreaterThan=1
}
public interface IUIConstraint
{
    cassowary.Constraint getConstraint(Dictionary<RectTransform, WrappedRect> dct);
}
[Serializable]
public class UIConnection: IUIConstraint
{
    public RectTransform first;
    public rectSide firstSide;
    public float firstcoef=1;
    public Relation relation;
    public RectTransform second;
    public rectSide secondSide;
    public float secondcoef=1;
    public double freecoef = 0;
    public cassowary.Rel strength=cassowary.Rel.Required;
    public cassowary.Constraint getConstraint(Dictionary<RectTransform, WrappedRect> dct)
    {
        WrappedRect f1;
        WrappedRect f2;
        cassowary.Expression ex1;
        cassowary.Expression ex2;
        if(first==null&&second==null)
        {
            return null;
        }
        if (first != null)
        {
            if (!dct.TryGetValue(first, out f1))
            {
                f1 = new WrappedRect(first);
                dct[first] = f1;
            }
            ex1 = f1.getTermFromSide(firstSide) * firstcoef;
        }
        else
            ex1 = new cassowary.Expression(firstcoef);
        if (second != null)
        {
            if (!dct.TryGetValue(second, out f2))
            {
                f2 = new WrappedRect(second);
                dct[second] = f2;
            }

            ex2 = f2.getTermFromSide(secondSide) * secondcoef;
        }
        else
            ex2= new cassowary.Expression(secondcoef);
        switch (relation)
        {
            case Relation.Equal:
                {
                    return cassowary.Constraint.Eq(ex1+freecoef, ex2) | cassowary.Strength.fromEnum(strength);
             
                }
            case Relation.GreaterThan:
                {
                    return cassowary.Constraint.Ge(ex1 + freecoef, ex2) | cassowary.Strength.fromEnum(strength);

                }
            case Relation.LessThan:
                {
                    return cassowary.Constraint.Le(ex1 + freecoef, ex2) | cassowary.Strength.fromEnum(strength);

                }
        }

        return null;
    }
}
public class WrappedRect
{
    public cassowary.Variable top;
    public cassowary.Variable right;
    public cassowary.Variable bottom;
    public cassowary.Variable left;
    public RectTransform linkedTransform;
    public WrappedRect(RectTransform linked)
    {
        linkedTransform = linked;
        top = new cassowary.Variable("top");
        right = new cassowary.Variable("right");
        bottom = new cassowary.Variable("bottom");
        left = new cassowary.Variable("left");

    }
    public int DistToCanvas(RectTransform canvas)
    {
        if (linkedTransform == canvas) return 0;
        int cnt = 0;
        Transform k = linkedTransform.transform;
        while (k!=null)
        {
            if (k.GetComponent<RectTransform>() == canvas)
                return cnt;
            cnt++;
            k = k.parent;
        }
         return cnt;
    }
    public cassowary.Expression getTermFromSide(rectSide side)
    {
        switch(side)
        {
            case rectSide.Bottom: return new cassowary.Term(bottom, 1);
            case rectSide.Top: return new cassowary.Term(top, 1);
            case rectSide.Left: return new cassowary.Term(left, 1);
            case rectSide.Right: return new cassowary.Term(right, 1);
            case rectSide.CenterX: return ( (cassowary.Expression)new cassowary.Term(left, 0.5)+ new cassowary.Term(right, 0.5));
            case rectSide.CenterY: return ((cassowary.Expression)new cassowary.Term(top, 0.5) + new cassowary.Term(bottom, 0.5));
            case rectSide.Width: return ((cassowary.Expression)new cassowary.Term(right, 1) - new cassowary.Term(left, 1));
            case rectSide.Height: return ((cassowary.Expression)new cassowary.Term(bottom, 1) - new cassowary.Term(top, 1));
        }
        return new cassowary.Expression();
    }
}

[ExecuteInEditMode]
public class SimplexCalc : MonoBehaviour
{
    public RectTransform canvas;
    public UIConnection[] connections;
    Dictionary<RectTransform, WrappedRect> vars=new Dictionary<RectTransform, WrappedRect>();
    cassowary.SolverImpl solver = new cassowary.SolverImpl();
    HashSet<cassowary.Constraint> cset = new HashSet<cassowary.Constraint>();
    Vector2 prevScreen = Vector2.zero;
    public Dictionary<RectTransform, WrappedRect> getVarsDict()
    {
        return vars;
    }
    public bool addConstraint(cassowary.Constraint c)
    {
        try { Debug.Log(c); solver.addConstraint(c); needs_reassign = true;  return true; } catch { return false; }
    }
    public bool removeConstraint(cassowary.Constraint c)
    {

        try { Debug.Log(c);  solver.removeConstraint(c); needs_reassign = true;  return true; } catch { return false; }
    }
    // Start is called before the first frame update
    void updateScreenConstraints()
    {
        if (canvas == null) return;

        
       
        solver.addConstraint(cassowary.Constraint.Eq(vars[canvas].getTermFromSide(rectSide.Top), 0));
        solver.addConstraint(cassowary.Constraint.Eq(vars[canvas].getTermFromSide(rectSide.Left), 0));
        cassowary.Constraint newx = cassowary.Constraint.Eq(vars[canvas].getTermFromSide(rectSide.Right), Screen.width);
        cassowary.Constraint newy = cassowary.Constraint.Eq(vars[canvas].getTermFromSide(rectSide.Bottom), Screen.height);
        try
        {
            solver.addConstraint(newx);
        }
        catch { }
        try
        {
            solver.addConstraint(newy);
        }
        catch { }

        prevScreen = new Vector2(Screen.width,Screen.height);
        Debug.Log(prevScreen);
        Debug.Log(Screen.width);
    }
    void Start()
    {
        if(canvas!=null&&!vars.ContainsKey(canvas))
        {
            vars[canvas] = new WrappedRect(canvas);
         
        }
        if(Screen.width!=prevScreen.x)
        {
            updateScreenConstraints();
        }
    }
    bool needs_rebuild = false;
    bool needs_reassign = false;
    void Rebuild()
    {
        solver.reset();
       
        if (canvas != null)
        {
            if ( !vars.ContainsKey(canvas))
            {
                vars[canvas] = new WrappedRect(canvas);
            }
            updateScreenConstraints();
            IUIConstraint[] cons = canvas.gameObject.GetComponentsInChildren<IUIConstraint>();
            foreach (IUIConstraint con in cons)
            {
                cassowary.Constraint st = con.getConstraint(vars);
                if (st == null) continue;
                try { solver.addConstraint(st); } catch { }
                
            }
           
            foreach (UIConnection con in connections)
            {
                cassowary.Constraint st = con.getConstraint(vars);
                try { solver.addConstraint(st); } catch { }
            }
            solver.updateVariables();
            needs_rebuild = true;
        }

}
    void OnValidate()
    {
        Debug.Log("validate");
        Rebuild();
        return;
        if (canvas != null && !vars.ContainsKey(canvas))
        {
            vars[canvas] = new WrappedRect(canvas);
            updateScreenConstraints();
        }
        solver.reset();
        updateScreenConstraints();
        if (Screen.width != prevScreen.x || Screen.height != prevScreen.y)
        {
            updateScreenConstraints();
        }
        HashSet<cassowary.Constraint> set2 = new HashSet<cassowary.Constraint>(cset);
        HashSet<cassowary.Constraint> toadd = new HashSet<cassowary.Constraint>();
        foreach (UIConnection con in connections)
        {
            cassowary.Constraint st = con.getConstraint(vars);
            solver.addConstraint(st);
            Debug.Log(st);

            if (st == null) continue;

            set2.Remove(st);
           
            if (!cset.Contains(st))
            {
                cset.Add(st);
                toadd.Add(st);
            }
        }
        foreach(cassowary.Constraint st in set2)
        {
            try
            {
               // solver.removeConstraint(st);
            }
            catch(ArgumentException)
            {

            }
            Debug.Log("Old constraint");
        }
        foreach (cassowary.Constraint st in toadd)
        {
            Debug.Log("New constraint");

            //solver.addConstraint(st);
        }
        solver.updateVariables();
        foreach(var kv in vars)
        {
            Debug.Log(kv.Value.linkedTransform);
           
            Debug.Log(kv.Value.left.value());
            Debug.Log(kv.Value.right.value());
            Debug.Log(kv.Value.top.value());
            Debug.Log(kv.Value.bottom.value());
            Debug.Log(Screen.height);
            Debug.Log(kv.Value.linkedTransform.anchorMax);
        }
    }
    public void triggerRebuild()
    {
        needs_rebuild = true;
    }
    // Update is called once per frame
    void Update()
    {
        if (Screen.width != prevScreen.x || Screen.height != prevScreen.y)
            needs_rebuild = true;
        //
        if (needs_rebuild)
        {
            Rebuild();
        }
        if(needs_reassign ||needs_rebuild)
        { 
            WrappedRect[] transfs = new WrappedRect[vars.Count];
            vars.Values.CopyTo(transfs, 0);
            Array.Sort<WrappedRect>(transfs, delegate (WrappedRect u1, WrappedRect u2)
            {
                return u1.DistToCanvas(canvas).CompareTo(u2.DistToCanvas(canvas));
            });
            foreach (var v in transfs)
            {

                if (v.linkedTransform != canvas)
                {
                    Vector3[] cor = new Vector3[4];
                    RectTransform par = v.linkedTransform.parent.GetComponent<RectTransform>();
                    if (par != null)
                    {
                        par.GetWorldCorners(cor);
                    }
                    else
                    {
                        canvas.GetWorldCorners(cor);
                    }

                    float wdth = cor[2].x - cor[0].x;
                    float height = cor[2].y - cor[0].y;
                    if (wdth == 0 || height == 0)
                    { continue; }
                    //Debug.Log(wdth);
                    //Debug.Log(height);
                    v.linkedTransform.offsetMax = new Vector2(0, 0);
                    v.linkedTransform.offsetMin = Vector2.zero;
                    float proper_y = Screen.height- cor[1].y;
                    v.linkedTransform.anchorMax = new Vector2((float)(v.right.value() - cor[0].x) / wdth, (float)(height +  proper_y -v.top.value() ) / height);
                    v.linkedTransform.anchorMin = new Vector2((float)(v.left.value() - cor[0].x) / wdth, (float)(height + proper_y - v.bottom.value() ) / height);
                    //Debug.LogFormat("Screen w:{0} h:{1}, tr: {2}, left: {3} right: {4} top: {5} bottom:{6}",Screen.width,Screen.height,v.linkedTransform,
                        //v.left.value(), v.right.value(), v.top.value(), v.bottom.value());
                    //Debug.LogFormat("Tr: {0} amin: {1} amax: {2} cor: {3} {4} {5} {6} proper:{7}",v.linkedTransform,v.linkedTransform.anchorMin,v.linkedTransform.anchorMax,cor[0],cor[1],cor[2],cor[3],proper_y);


                }

            }
            needs_rebuild = false;
            needs_reassign = false;
        }
    }
}
