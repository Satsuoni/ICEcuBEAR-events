using System.Collections.Generic;
namespace cassowary
{
public class Expression
{


    List<Term> m_terms=new List<Term>();
    double m_constant;
    
    public Expression(double constant = 0.0) 
    {
         m_constant =constant;
    }
        public override string ToString()
        {
            string ret = "Ex( ";
            foreach(Term t in m_terms)
            {
                ret += t.ToString();
            }
            ret += string.Format("+ {0})", m_constant);
            return ret;
        }

        public  Expression(Term term, double constant = 0.0) 
  { 
      m_terms=new List<Term>();
      m_terms.Add(term);
      m_constant=constant;
}

   public Expression(List<Term> terms, double constant = 0.0) 
   {
    m_terms=terms;
    m_constant=constant;
   }

   public  List<Term> terms()
    {
        return m_terms;
    }

  public   double constant() 
    {
        return m_constant;
    }

   public  double value() 
    {
        double result = m_constant;
        foreach(Term t in m_terms)
        {
         result+=t.value();
        }
    
        return result;
    }

        public static bool operator ==(Expression lhs, Expression rhs)
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

        public static bool operator !=(Expression lhs, Expression rhs)
        {
            return !(lhs == rhs);
        }
        public override bool Equals(object o)
        {
            if (o == null)
                return false;

            var second = o as Expression;
            if (second == null) return false;
            if (second.m_terms.Count != m_terms.Count) return false;
            if(second.m_constant!=m_constant) return  false;
            var en = second.m_terms.GetEnumerator();
            en.MoveNext();
            foreach (Term t in m_terms)
            {
                Term e = en.Current;
                if (t != e) return false;
                en.MoveNext();
            }

            return true;
        }


        public override int GetHashCode()
        {
            int hc = 12345;
            int cnt = 0;
            foreach (Term t in m_terms)
            {
                hc ^= Util.ShiftAndWrap(t.GetHashCode(), cnt);
                cnt++;
            }
            hc ^= Util.ShiftAndWrap(m_constant.GetHashCode(), cnt);
            return hc;
        }


        public static Expression operator+( Expression first,  Expression second )
{
    List<Term> fl=first.terms();
    List<Term> sc=second.terms();
    List<Term> terms=new List<Term>();
    foreach(Term t in fl)
    {
terms.Add(t);
    }
    foreach(Term t in sc)
    {
terms.Add(t);
    }

	return new  Expression( terms, first.constant() + second.constant() );
}
public static Expression operator+(  Expression first,  Term second )
{
     List<Term> fl=first.terms();
      List<Term> terms=new List<Term>();
    foreach(Term t in fl)
    {
terms.Add(t);
    }
    terms.Add(second);
	
	return new Expression( terms, first.constant() );
}
public static Expression operator+(  Expression expression,  Variable variable )
{
	return expression + (Term) variable ;
}
public static  Expression operator+(  Expression expression, double constant )
{
	return  new Expression( expression.terms(), expression.constant() + constant );
}

public static Expression operator*(  Expression expression, double coefficient )
{
	List<Term> terms=new List<Term>();
      foreach(Term t in expression.terms())
    {
    terms.Add(t*coefficient);
    }
	return new Expression( terms, expression.constant() * coefficient );
}

public static Expression operator-(  Expression expression )
{
	return expression * -1.0;
}


public static Expression operator -(  Expression first,  Expression second )
{
	return first + -second;
}

public static Expression operator-(  Expression expression,  Term term )
{
	return expression + -term;
}


public static Expression operator-(  Expression expression,  Variable variable )
{
	return expression + new Term(variable,-1);
}



public static Expression operator-(  Expression expression, double constant )
{
	return expression + -constant;
}
public static Expression operator/(  Expression expression, double denominator )
{
	return expression * ( 1.0 / denominator );
}


public static Expression operator*( double coefficient,  Expression expression )
{
	return expression * coefficient;
}

public static Expression operator+(  Term term,  Expression expression )
{
	return expression + term;
}

public static implicit operator Expression(Term d) 
{
	return new Expression(d,0.0);
}
public static  Expression operator -(  Term term, Expression expression )
{
	return -expression + term;
}
public static Expression operator+(  Variable variable,  Expression expression )
{
	return expression + variable;
}
public static Expression operator-(  Variable variable,  Expression expression )
{
	return variable + -expression;
}

public static  Expression operator+( double constant,  Expression expression )
{
	return expression + constant;
}
public static  Expression operator-( double constant,  Expression expression )
{
	return -expression + constant;
}

}

}