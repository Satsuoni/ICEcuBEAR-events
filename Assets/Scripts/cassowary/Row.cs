using System.Collections.Generic;
namespace cassowary
{

using CellMap=Dictionary<Symbol,double>;

class Row
{
CellMap m_cells;
    double m_constant;

    public Row()  
    {
        m_constant=0.0;
        m_cells=new CellMap();
    }

    public Row(double constant) 
    {
        m_constant=constant;
         m_cells=new CellMap();
    }

   public Row( Row other) 
   {
        m_cells=other.m_cells;
        m_constant= other.m_constant;
   }


   public  CellMap cells()
    {
        return m_cells;
    }

   public double constant() 
    {
        return m_constant;
    }

    /* Add a constant value to the row constant.
	The new value of the constant is returned.
	*/
    public double add(double value)
    {
        return m_constant += value;
    }

    /* Insert a symbol into the row with a given coefficient.
	If the symbol already exists in the row, the coefficient will be
	added to the existing coefficient. If the resulting coefficient
	is zero, the symbol will be removed from the row.
	*/
    public void insert( Symbol symbol, double coefficient = 1.0)
    {
        if(m_cells.ContainsKey(symbol))
        {
            m_cells[symbol] += coefficient;
        }
        else
        {
            m_cells[symbol] = coefficient;
        }
        if (Util.nearZero(m_cells[symbol]))
            m_cells.Remove(symbol);
    }

    /* Insert a row into this row with a given coefficient.
	The constant and the cells of the other row will be multiplied by
	the coefficient and added to this row. Any cell with a resulting
	coefficient of zero will be removed from the row.
	*/
   public  void insert( Row other, double coefficient = 1.0)
    {
      //  typedef CellMap::const_iterator iter_t;
        m_constant += other.m_constant * coefficient;
        foreach(KeyValuePair<Symbol,double> kv in other.cells())
        {
           insert(kv.Key,kv.Value*coefficient);
        }
     
    }

    /* Remove the given symbol from the row.
	*/
   public  void remove( Symbol symbol)
    {
        m_cells.Remove(symbol);
   
    }

    /* Reverse the sign of the constant and all cells in the row.
	*/
   public  void reverseSign()
    {
        m_constant = -m_constant;
        List<Symbol> keys=new List<Symbol>();
        foreach(Symbol sv in m_cells.Keys)
        {
        keys.Add(sv);
        }
        foreach(Symbol sv in keys)
        {
        m_cells[sv]=-m_cells[sv];
        }
     
    }

    /* Solve the row for the given symbol.
	This method assumes the row is of the form a * x + b * y + c = 0
	and (assuming solve for x) will modify the row to represent the
	right hand side of x = -b/a * y - c / a. The target symbol will
	be removed from the row, and the constant and other cells will
	be multiplied by the negative inverse of the target coefficient.
	The given symbol *must* exist in the row.
	*/
   public  void solveFor( Symbol symbol)
    {
        double coeff = -1.0 / m_cells[symbol];
        m_cells.Remove(symbol);
        m_constant *= coeff;
        List<Symbol> keys=new List<Symbol>();
        foreach(Symbol sv in m_cells.Keys)
        {
        keys.Add(sv);
        }
        foreach(Symbol sv in keys)
        {
            m_cells[sv]*=coeff;
        }

    }

    /* Solve the row for the given symbols.
	This method assumes the row is of the form x = b * y + c and will
	solve the row such that y = x / b - c / b. The rhs symbol will be
	removed from the row, the lhs added, and the result divided by the
	negative inverse of the rhs coefficient.
	The lhs symbol *must not* exist in the row, and the rhs symbol
	*must* exist in the row.
	*/
   public void solveFor( Symbol lhs, Symbol rhs)
    {
        insert(lhs, -1.0);
        solveFor(rhs);
    }

    /* Get the coefficient for the given symbol.
	If the symbol does not exist in the row, zero will be returned.
	*/
   public  double coefficientFor( Symbol symbol) 
    {
            double ret;
            if(m_cells.TryGetValue(symbol,out ret))
            {
                return ret;
            }
            return 0.0;
    
    }

    /* Substitute a symbol with the data from another row.
	Given a row of the form a * x + b and a substitution of the
	form x = 3 * y + c the row will be updated to reflect the
	expression 3 * a * y + a * c + b.
	If the symbol does not exist in the row, this is a no-op.
	*/
    public void substitute( Symbol symbol,  Row row)
    {
        double coefficient;
        if(m_cells.TryGetValue(symbol,out coefficient))
        {
           m_cells.Remove(symbol);
             insert(row, coefficient);
        }
      
    }


    
}


} // namespace cassowary