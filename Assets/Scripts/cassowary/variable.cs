using System;

namespace cassowary
{
    
public static class Util
{
public static  bool nearZero(double value)
{
    const double eps = 1.0e-8;
    return value < 0.0 ? -value < eps : value < eps;
}
        public static int ShiftAndWrap(int value, int positions)
        {
            positions = positions & 0x1F;

            // Save the existing bit pattern, but interpret it as an unsigned integer.
            uint number = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);
            // Preserve the bits to be discarded.
            uint wrapped = number >> (32 - positions);
            // Shift and wrap the discarded bits.
            return BitConverter.ToInt32(BitConverter.GetBytes((number << positions) | wrapped), 0);
        }
    }


public class Variable
{
      

        public class VariableData 
    {



  

        public string m_name;
       
        public double m_value;
public VariableData(string nm)
{
    m_name=nm;
    m_value=0;
}

    };

   VariableData m_data;
        public override string ToString()
        {
            return string.Format("({0})", m_data.m_name);
        }
        public   Variable() 
    {
    m_data=new VariableData("");
    }

    public Variable(string name ) 
    {
        m_data=new VariableData(name); 
    }



    public string name() 
    {
        return m_data.m_name;
    }

    public void setName(string name)
    {
        m_data.m_name = name;
    }

 


    public double value() 
    {
        return m_data.m_value;
    }

    public void setValue(double value)
    {
        m_data.m_value = value;
    }

    // operator== is used for symbolics
    public  bool equals( Variable other)
    {
        return m_data == other.m_data;
    }


    public static bool operator < ( Variable lhs,  Variable rhs)
    {
        return lhs.m_data.m_value < rhs.m_data.m_value;
    }
      public static bool operator > ( Variable lhs,  Variable rhs)
    {
        return lhs.m_data.m_value > rhs.m_data.m_value;
    }

}
}