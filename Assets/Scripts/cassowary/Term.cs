using System;
namespace cassowary
{
public class Term
{

	Variable m_variable;
	double m_coefficient;

public	Term(  Variable variable, double coefficient = 1.0 ) 
{
m_variable=variable;
m_coefficient=coefficient;

}

        public override string ToString()
        {
            return string.Format("({0}*{1})", m_coefficient, m_variable);
        }

        // to facilitate efficient map -> vector copies
        public	Term(System.Collections.Generic.KeyValuePair<Variable, double> pair ) 
{
    m_variable=pair.Key;
    m_coefficient=pair.Value;
}
	
	public  Variable variable()
	{
		return m_variable;
	}

	public double coefficient() 
	{
		return m_coefficient;
	}

	public double value() 
	{
		return m_coefficient * m_variable.value();
	}

        public static bool operator ==(Term lhs, Term rhs)
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

        public static bool operator !=(Term lhs, Term rhs)
        {
            return !(lhs == rhs);
        }
        public override bool Equals(object o)
        {
            if (o == null)
                return false;

            var second = o as Term;
            if (second == null) return false;
            if (second.m_coefficient != m_coefficient) return false;
            if (second.m_variable != m_variable) return false;
           
            return true;
        }


        public override int GetHashCode()
        {
            int hc = 4572;
           
            
            hc ^= Util.ShiftAndWrap(m_variable.GetHashCode(), 5);
            hc ^= Util.ShiftAndWrap(m_coefficient.GetHashCode(), 12);
            return hc;
        }

        public static implicit operator Term(Variable d) 
{
	return new Term(d,1.0);
}

public static Term operator*(  Term variable, double coefficient )
{
	return new Term( variable.variable(), coefficient*variable.coefficient() );
}

public static Term operator*( double coefficient,  Term term )
{
	return term * coefficient;
}



public static  Term operator/( Term variable, double denominator )
{
	return variable * ( 1.0 / denominator );
}



public static Term operator-(  Term variable )
{
	return variable * -1.0;
}




};
}