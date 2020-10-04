using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class CToggle
{
    public string name;
    public Relation relation;
    public cassowary.Rel strength = cassowary.Rel.Required;
    public SingleTerm[] terms;
    public cassowary.Constraint getConstraint(Dictionary<RectTransform, WrappedRect> dct)
    {
        cassowary.Expression ex = new cassowary.Expression();
        foreach (SingleTerm term in terms)
        {
            if (term.rect == null)
            {
                ex = ex + new cassowary.Expression(term.coef);
            }
            else
            {
                WrappedRect f1;
                if (!dct.TryGetValue(term.rect, out f1))
                {
                    f1 = new WrappedRect(term.rect);
                    dct[term.rect] = f1;
                }
                ex = ex + f1.getTermFromSide(term.side) * term.coef;
            }
        }

        switch (relation)
        {
            case Relation.Equal:
                {
                    return cassowary.Constraint.Eq(ex, 0) | cassowary.Strength.fromEnum(strength);
                }
            case Relation.GreaterThan:
                {
                    return cassowary.Constraint.Ge(ex, 0) | cassowary.Strength.fromEnum(strength);
                }
            case Relation.LessThan:
                {
                    return cassowary.Constraint.Le(ex, 0) | cassowary.Strength.fromEnum(strength);
                }
        }

        return null;
    }
}

public class ToggleConstraint : MonoBehaviour,IUIConstraint
{
    public int current=0;
    public CToggle[] toggles;
    SimplexCalc solver;
    // Start is called before the first frame update
    void Start()
    {
        solver = FindObjectOfType<SimplexCalc>();
    }
    public void ToggleByIndex(int index)
    {
        if (solver==null)
            solver = FindObjectOfType<SimplexCalc>();
        if (solver == null) return;
        if(index<0||index>=toggles.Length)
        {
            Debug.LogFormat("Invalid toggle index {0}", index);
            return;
        }
        //cassowary.Constraint oldC = toggles[current].getConstraint(solver.getVarsDict());
       // cassowary.Constraint newC = toggles[index].getConstraint(solver.getVarsDict());
        //solver.removeConstraint(oldC);
        //solver.addConstraint(newC);
        solver.triggerRebuild();
        current = index;

    }
    public void ToggleByName(string name)
    {
        for(int i=0;i<toggles.Length;i++)
        {
            if(toggles[i].name==name)
            {
                ToggleByIndex(i);
                return;
            }
        }
    }
    public cassowary.Constraint getConstraint(Dictionary<RectTransform, WrappedRect> dct)
    {
        if (current < 0 || current >= toggles.Length) return null;
        return toggles[current].getConstraint(dct);
    }
    // Update is called once per frame
   
}
