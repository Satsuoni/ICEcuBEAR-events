using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SingleTerm
{
    public RectTransform rect;
    public rectSide side;
    public double coef=0;
}
public class ConstraintTerm : MonoBehaviour, IUIConstraint
{
    public Relation relation;
    public cassowary.Rel strength = cassowary.Rel.Required;
    public SingleTerm[] terms;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public cassowary.Constraint getConstraint(Dictionary<RectTransform, WrappedRect> dct)
    {
        cassowary.Expression ex=new cassowary.Expression();
        foreach(SingleTerm term in terms)
        {
            if (term.rect==null)
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
                ex=ex + f1.getTermFromSide(term.side) * term.coef;
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
