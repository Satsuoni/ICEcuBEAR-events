using System;
using System.Collections.Generic;


namespace cassowary
{

public enum RelationalOperator
{
    OP_LE,
    OP_GE,
    OP_EQ
};

public class Constraint
{
      public  static Expression reduce( Expression expr)
    {
        Dictionary<Variable,double> vars=new Dictionary<Variable, double>();
        foreach(Term t in expr.terms())
        {
            if(vars.ContainsKey(t.variable()))
            {
           vars[t.variable()]+=t.coefficient();
            }
          else
          {
              vars[t.variable()]=t.coefficient();
          }
        }
        List<Term> terms=new List<Term>();
        foreach(KeyValuePair<Variable,double> kv in vars )
        {
            terms.Add(new Term(kv.Key,kv.Value));
        } 
        return new Expression(terms, expr.constant());
    }

  public class ConstraintData 
    {

      public   Expression m_expression;
      public   double m_strength;
      public   RelationalOperator m_op;
            public override string ToString()
            {
                return string.Format("Constraint({0},{1},{2})",m_expression,m_op,m_strength);
            }
            public ConstraintData(Constraint other, double strength)
            {
                m_expression = other.expression();
                m_strength = Strength.clip(strength);
                m_op = other.op();

            }
            public  ConstraintData( Expression expr,
                       RelationalOperator op,
                       double strength) 
                       {
                         m_expression=Constraint.reduce(expr);
                         m_strength=Strength.clip(strength);
                         m_op=op;
                       }

            public static bool operator ==(ConstraintData lhs, ConstraintData rhs)
            {
                if (ReferenceEquals(lhs, rhs))
                {
                    return true;
                }
                if (ReferenceEquals(lhs, null))
                {
                    return false;
                }

                return lhs.Equals(rhs);
            }

            public static bool operator !=(ConstraintData lhs, ConstraintData rhs)
            {
                return !(lhs == rhs);
            }
            public override bool Equals(object o)
            {
                if (o == null)
                    return false;

                var second = o as ConstraintData;
                return second != null && m_strength == second.m_strength && m_op==second.m_op && m_expression==second.m_expression;
            }

          
            public override int GetHashCode()
            {
               
                return Util.ShiftAndWrap(m_strength.GetHashCode(),5)^ m_op.GetHashCode()^ Util.ShiftAndWrap(m_expression.GetHashCode(), 12);
            }
        };
public ConstraintData m_data;
public     Constraint()  {
    m_data=null;
}
        public override string ToString()
        {
            return m_data.ToString();
         }
        public   Constraint( Expression expr, RelationalOperator op, double strength = Strength.reqq)  
 {
     m_data= new ConstraintData(expr, op, strength);
 }

   public Constraint( Constraint other, double strength) 
   {
       m_data=new ConstraintData(other, strength);
   }



    public  Expression expression() 
    {
        return m_data.m_expression;
    }

    public RelationalOperator op() 
    {
        return m_data.m_op;
    }

    public double strength() 
    {
        return m_data.m_strength;
    }

    public static bool operator!(Constraint cs) 
    {
        return !(cs.m_data==null);
    }


    /*public static  bool operator<( Constraint lhs,  Constraint rhs)
    {
        return lhs.m_data < rhs.m_data;
    }*/

      public static bool operator==( Constraint lhs,  Constraint rhs)
    {
            if (ReferenceEquals(lhs, rhs))
            {
                return true;
            }
            if (ReferenceEquals(lhs, null))
            {
                return false;
            }

            return lhs.Equals(rhs);
    }

     public static bool operator!=( Constraint lhs,  Constraint rhs)
    {
        return !(lhs == rhs);
    }
    public override bool Equals(object o)
{
   if(o == null)
       return false;

   var second = o as Constraint;

   return second != null && m_data == second.m_data;
}
public override int GetHashCode()
{
    return m_data.GetHashCode();
}


public static Constraint Eq( Expression first,  Expression second )
{
	return new  Constraint( first - second,RelationalOperator.OP_EQ );
}
public static Constraint Eq(  Expression expression,  Term term )
{
	return Eq(expression,new Expression( term ));
}

public static Constraint Eq(  Expression expression,  Variable variable )
{
	return Eq(expression, new Term( variable ));
}

public static Constraint Eq(  Expression expression, double constant )
{
	return Eq(expression,new Expression( constant ));
}

public static Constraint Eq(  Term term,  Expression expression )
{
	return Eq(expression, term);
}



public static Constraint Eq(  Term first,  Term second )
{
	return Eq(new Expression( first ), second);
}



public static Constraint Eq(  Term term,  Variable variable )
{
	return Eq(new Expression( term ) , variable);
}




public static Constraint Eq(  Term term, double constant )
{
	return Eq(new Expression( term ), constant);
}




public static Constraint Le(  Expression first,  Expression second )
{
	return new Constraint( first - second, RelationalOperator.OP_LE );
}
public static Constraint Le(  Expression expression,  Term term )
{
	return Le(expression,new  Expression( term ));
}


public static Constraint Le(  Expression expression,  Variable variable )
{
	return Le(expression, new Term( variable ));
}


public static Constraint Le(  Expression expression, double constant )
{
	return Le(expression, new Expression( constant ));
}

public static Constraint  Ge(  Expression first,  Expression second )
{
	return new Constraint( first - second, RelationalOperator.OP_GE );
}




public static Constraint  Ge(  Expression expression,  Term term )
{
	return Ge(expression ,new  Expression( term ));
}



public static Constraint  Ge (  Expression expression,  Variable variable )
{
	return Ge(expression,new Term( variable ));
}



public static Constraint  Ge (  Expression expression, double constant )
{
	return Ge(expression,new  Expression( constant ));
}
public static Constraint Le(  Term first,  Term second )
{
	return Le(new Expression( first ) , second);
}



public static Constraint Le(  Term term,  Variable variable )
{
	return Le(new  Expression( term ) , variable);
}



public static Constraint Le(  Term term, double constant )
{
	return Le(new Expression( term ), constant);
}
public static Constraint Le(  Term term,  Expression expression )
{
	return Ge( expression , term);
}




public static Constraint  Ge(  Term term,  Expression expression )
{
	return Le(expression , term);
}



public static Constraint  Ge(  Term first, Term second )
{
	return Ge(new  Expression( first ) , second);
}



public static Constraint  Ge(  Term term, Variable variable )
{
	return Ge(new Expression( term ) , variable);
}


public static Constraint  Ge(  Term term, double constant )
{
	return Ge(new Expression( term ), constant) ;
}


public static Constraint operator|(  Constraint constraint, double strength )
{
	return new Constraint( constraint, strength );
}
public static Constraint  operator|( double strength,  Constraint constraint )
{
	return constraint | strength;
}

};

} // namespace kiwi