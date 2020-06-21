using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SelfConstraint : MonoBehaviour, IUIConstraint
{
    RectTransform first;
    public rectSide firstSide;
    public float firstcoef = 1;
    public Relation relation;
    public RectTransform second;
    public rectSide secondSide;
    public float secondcoef = 1;
    public double freecoef = 0;
    public cassowary.Rel strength = cassowary.Rel.Required;
    // Start is called before the first frame update
    void Start()
    {
        first = gameObject.GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public cassowary.Constraint getConstraint(Dictionary<RectTransform, WrappedRect> dct)
    {
        if(first==null)
            first = gameObject.GetComponent<RectTransform>();
        WrappedRect f1;
        WrappedRect f2;
        cassowary.Expression ex1;
        cassowary.Expression ex2;
        if (first == null && second == null)
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
            ex2 = new cassowary.Expression(secondcoef);
        switch (relation)
        {
            case Relation.Equal:
                {
                    return cassowary.Constraint.Eq(ex1 + freecoef, ex2) | cassowary.Strength.fromEnum(strength);
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
