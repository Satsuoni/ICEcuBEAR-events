using System; 
namespace cassowary
{
using  Id=System.UInt64;
public class Symbol
{

	public enum Type
	{
		Invalid,
		External,
		Slack,
		Error,
		Dummy
	};

	Id m_id;
	Type m_type;

	public Symbol() 
    {
        m_id=0;
        m_type=Type.Invalid;
    }
        public override string ToString()
        {
            return string.Format( "Symbol<{0}, {1}>",m_id,m_type);
        }

        public Symbol( Type type, Id id )  
    {
        m_id= id;
        m_type= type ;

    }
	public Symbol( Symbol other )  
    {
        m_id=other.id();
        m_type= other.type() ;

    }
	
	public Id id() 
	{
		return m_id;
	}

	public Type type() 
	{
		return m_type;
	}


public static bool  operator<(  Symbol lhs,  Symbol rhs )
	{
		return lhs.m_id < rhs.m_id;
	}
public static bool  operator>(  Symbol lhs,  Symbol rhs )
	{
		return lhs.m_id > rhs.m_id;
	}

	public static bool operator==(  Symbol lhs, Symbol rhs )
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
	public static bool operator!=(  Symbol lhs, Symbol rhs )
	{
          
		return !(lhs == rhs);
	}
       public override bool Equals(object o)
{
   if(o == null)
       return false;

   var second = o as Symbol;

   return second != null && m_id == second.m_id;
}
public override int GetHashCode()
{
    return m_id.GetHashCode();
}

}


} // namespace kiwi