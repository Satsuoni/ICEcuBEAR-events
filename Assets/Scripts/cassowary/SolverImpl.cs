using System;
using System.Collections.Generic;

namespace cassowary
{
	using  VarMap= Dictionary<Variable, Symbol>;
    using RowMap=Dictionary<Symbol,Row>;
	using CnMap=Dictionary<Constraint,Tag>;
    using EditMap=Dictionary<Variable,EditInfo>;
    using  Id=System.UInt64;
	public class Tag
	{
		public Symbol marker;
		public Symbol other;
        public Tag()
        {
            marker=new Symbol();
            other=new Symbol ();
        }
	}

	public class EditInfo
	{
		public Tag tag=new Tag();
		public Constraint constraint;
		public double constant;
	}


public class SolverImpl
{


	CnMap m_cns=new CnMap();
	RowMap m_rows=new RowMap();
	VarMap m_vars=new VarMap();
	EditMap m_edits=new EditMap();
	List<Symbol> m_infeasible_rows=new List<Symbol>();
	Row m_objective;
	Row m_artificial;
	Id m_id_tick;




	public SolverImpl()   
    {
        m_objective= new Row();
        m_infeasible_rows=new List<Symbol>();
         m_id_tick= 1 ;
    }

	//~SolverImpl() { clearRows(); }

	/* Add a constraint to the solver.

	Throws
	------
	DuplicateConstraint
		The given constraint has already been added to the solver.

	UnsatisfiableConstraint
		The given constraint is required and cannot be satisfied.

	*/
	public void addConstraint(  Constraint constraint )
	{
		if( m_cns.ContainsKey( constraint ) )
			throw new System.ArgumentException("DuplicateConstraint", "SolverImpl");// constraint? 

		// Creating a row causes symbols to be reserved for the variables
		// in the constraint. If this method exits with an exception,
		// then its possible those variables will linger in the var map.
		// Since its likely that those variables will be used in other
		// constraints and since exceptional conditions are uncommon,
		// i'm not too worried about aggressive cleanup of the var map.
		Tag tag=new Tag();
		Row rowptr= createRow( constraint, tag ) ;
		Symbol subject=  chooseSubject( rowptr, tag ) ;

		// If chooseSubject could not find a valid entering symbol, one
		// last option is available if the entire row is composed of
		// dummy variables. If the constant of the row is zero, then
		// this represents redundant constraints and the new dummy
		// marker can enter the basis. If the constant is non-zero,
		// then it represents an unsatisfiable constraint.
		if( subject.type() == Symbol.Type.Invalid && allDummies( rowptr ) )
		{
			if( !Util.nearZero( rowptr.constant() ) )
				throw new System.ArgumentException("UnsatisfiableConstraint", "SolverImpl"); //UnsatisfiableConstraint( constraint );
			else
				subject = tag.marker;
		}

		// If an entering symbol still isn't found, then the row must
		// be added using an artificial variable. If that fails, then
		// the row represents an unsatisfiable constraint.
		if( subject.type() == Symbol.Type.Invalid )
		{
			if( !addWithArtificialVariable( rowptr ) )
				throw new System.ArgumentException("UnsatisfiableConstraint", "SolverImpl");//throw UnsatisfiableConstraint( constraint );
		}
		else
		{
			rowptr.solveFor( subject );
			substitute( subject, rowptr );
			m_rows[ subject ] = rowptr;
		}

		m_cns[ constraint ] = tag;

		// Optimizing after each constraint is added performs less
		// aggregate work due to a smaller average system size. It
		// also ensures the solver remains in a consistent state.
		optimize( m_objective );
	}

	/* Remove a constraint from the solver.

	Throws
	------
	UnknownConstraint
		The given constraint has not been added to the solver.

	*/
	public void removeConstraint(  Constraint constraint )
	{
        if(!m_cns.ContainsKey(constraint))
        {
           throw new System.ArgumentException("UnknownConstraint", "SolverImpl"); //throw UnknownConstraint( constraint );
        }

		Tag tag=new Tag( );
        tag.marker=m_cns[constraint].marker;
        tag.other=m_cns[constraint].other;
		m_cns.Remove( constraint );

		// Remove the error effects from the objective function
		// *before* pivoting, or substitutions into the objective
		// will lead to incorrect solver results.
		removeConstraintEffects( constraint, tag );

		// If the marker is basic, simply drop the row. Otherwise,
		// pivot the marker into the basis and then drop the row.
		//RowMap::iterator row_it = m_rows.find( tag.marker );


		if( m_rows.ContainsKey(tag.marker))
		{
			
			m_rows.Remove( tag.marker  );
		}
		else
		{
			KeyValuePair<Symbol,Row> row_it = getMarkerLeavingRow( tag.marker );
			if( row_it.Key == null )
				throw new System.ArgumentException("InternalSolverError: failed to find leaving row", "SolverImpl"); // InternalSolverError( "failed to find leaving row" );
			Symbol leaving=  row_it.Key ;
			Row rowptr=  row_it.Value ;
			m_rows.Remove( leaving );
			rowptr.solveFor( leaving, tag.marker );
			substitute( tag.marker, rowptr );
		}

		// Optimizing after each constraint is removed ensures that the
		// solver remains consistent. It makes the solver api easier to
		// use at a small tradeoff for speed.
		optimize( m_objective );
	}

	/* Test whether a constraint has been added to the solver.

	*/
	public bool hasConstraint(  Constraint constraint ) 
	{
		return m_cns.ContainsKey(constraint);
	}

	/* Add an edit variable to the solver.

	This method should be called before the `suggestValue` method is
	used to supply a suggested value for the given edit variable.

	Throws
	------
	DuplicateEditVariable
		The given edit variable has already been added to the solver.

	BadRequiredStrength
		The given strength is >= required.

	*/
	public void addEditVariable(  Variable variable, double strength )
	{
        if(m_edits.ContainsKey(variable))
         throw new System.ArgumentException("DuplicateEditVariable", "SolverImpl");
		strength = Strength.clip( strength );
		if( strength == Strength.required )
			throw new System.ArgumentException("BadRequiredStrength", "SolverImpl");//BadRequiredStrength();
		Constraint cn = new Constraint(new Expression( variable ),  RelationalOperator.OP_EQ, strength );
		addConstraint( cn );
		EditInfo info=new EditInfo();
		info.tag = m_cns[ cn ];
		info.constraint = cn;
		info.constant = 0.0;
		m_edits[ variable ] = info;
	}

	/* Remove an edit variable from the solver.

	Throws
	------
	UnknownEditVariable
		The given edit variable has not been added to the solver.

	*/
	public void removeEditVariable(  Variable variable )
	{
        if(!m_edits.ContainsKey(variable))
          throw new System.ArgumentException("UnknownEditVariable", "SolverImpl");
		removeConstraint( m_edits[variable].constraint );
		m_edits.Remove( variable );
	}

	/* Test whether an edit variable has been added to the solver.

	*/
	public bool hasEditVariable(  Variable variable ) 
	{
		return m_edits.ContainsKey(variable);
	}

	/* Suggest a value for the given edit variable.

	This method should be used after an edit variable as been added to
	the solver in order to suggest the value for that variable.

	Throws
	------
	UnknownEditVariable
		The given edit variable has not been added to the solver.

	*/
	public void suggestValue(  Variable variable, double value )
	{
        if(!m_edits.ContainsKey(variable))
          throw new System.ArgumentException("UnknownEditVariable", "SolverImpl");


		//DualOptimizeGuard guard( *this );
		EditInfo info = m_edits[variable];
		double delta = value - info.constant;
		info.constant = value;

		// Check first if the positive error variable is basic.
	  Row trial;
		if( m_rows.TryGetValue(info.tag.marker,out trial) )
		{
			if( trial.add( -delta ) < 0.0 )
				m_infeasible_rows.Add(info.tag.marker );
            dualOptimize();
			return;
		}

		// Check next if the negative error variable is basic.
		if( m_rows.TryGetValue(info.tag.other,out trial)  )
		{
			if( trial.add( delta ) < 0.0 )
				m_infeasible_rows.Add( info.tag.other  );
               dualOptimize();
			return;
		}
        // Otherwise update each row where the error variables exist.
        foreach(KeyValuePair<Symbol,Row> r in m_rows)
        {
          double coeff = r.Value.coefficientFor( info.tag.marker );
         if( coeff != 0.0 &&
				r.Value.add( delta * coeff ) < 0.0 &&
				r.Key.type() != Symbol.Type.External )
				m_infeasible_rows.Add(r.Key );
        }
		dualOptimize();
	}

	/* Update the values of the external solver variables.

	*/
	public void updateVariables()
	{
	
        foreach(KeyValuePair<Variable,Symbol> vr in m_vars)
        {
            Row trial;
            if(m_rows.TryGetValue(vr.Value,out trial))
            {
             vr.Key.setValue(trial.constant());
            }
            else
            {
                vr.Key.setValue(0);
            }
        }
		
	}

	/* Reset the solver to the empty starting condition.

	This method resets the internal solver state to the empty starting
	condition, as if no constraints or edit variables have been added.
	This can be faster than deleting the solver and creating a new one
	when the entire system must change, since it can avoid unecessary
	heap (de)allocations.

	*/
	public void reset()
	{
		clearRows();
		m_cns.Clear();
		m_vars.Clear();
		m_edits.Clear();
		m_infeasible_rows.Clear();
		m_objective= new Row() ;
		m_artificial=null;
		m_id_tick = 1;
	}





	void clearRows()
	{
	//	std::for_each( m_rows.begin(), m_rows.end(), RowDeleter() );
		m_rows.Clear();
	}

	/* Get the symbol for the given variable.

	If a symbol does not exist for the variable, one will be created.

	*/
	Symbol getVarSymbol(  Variable variable )
	{
       Symbol sym;
       if(m_vars.TryGetValue(variable,out sym))
       {
           return sym;
       }
		Symbol symbol=new Symbol( Symbol.Type.External, m_id_tick++ );
		m_vars[ variable ] = symbol;
		return symbol;
	}

	/* Create a new Row object for the given constraint.

	The terms in the constraint will be converted to cells in the row.
	Any term in the constraint with a coefficient of zero is ignored.
	This method uses the `getVarSymbol` method to get the symbol for
	the variables added to the row. If the symbol for a given cell
	variable is basic, the cell variable will be substituted with the
	basic row.

	The necessary slack and error variables will be added to the row.
	If the constant for the row is negative, the sign for the row
	will be inverted so the constant becomes positive.

	The tag will be updated with the marker and error symbols to use
	for tracking the movement of the constraint in the tableau.

	*/
	Row createRow(  Constraint constraint, Tag tag )
	{
		Expression expr= constraint.expression();
		Row row = new Row( expr.constant() );
        // Substitute the current basic variables into the row.
        foreach(Term t in expr.terms() )
        {
           if( !Util.nearZero( t.coefficient() ) )
			{
				Symbol symbol=new Symbol ( getVarSymbol( t.variable() ) );
                Row tr;
                if(m_rows.TryGetValue(symbol,out tr))
                {
                 row.insert(tr,t.coefficient());
                }
                else
                {
                    row.insert(symbol,t.coefficient());
                }
			} 
        }
		

		// Add the necessary slack, error, and dummy variables.
		switch( constraint.op() )
		{
			case RelationalOperator.OP_LE:
			case RelationalOperator.OP_GE:
			{
				double coeff = constraint.op() == RelationalOperator.OP_LE ? 1.0 : -1.0;
				Symbol slack=new Symbol( Symbol.Type.Slack, m_id_tick++ );
				tag.marker = slack;
				row.insert( slack, coeff );
				if( constraint.strength() < Strength.required )
				{
					Symbol error=new Symbol( Symbol.Type.Error, m_id_tick++ );
					tag.other = error;
					row.insert( error, -coeff );
					m_objective.insert( error, constraint.strength() );
				}
				break;
			}
			case RelationalOperator.OP_EQ:
			{
				if( constraint.strength() < Strength.required )
				{
					Symbol errplus=new Symbol( Symbol.Type.Error, m_id_tick++ );
					Symbol errminus=new Symbol ( Symbol.Type.Error, m_id_tick++ );
					tag.marker = errplus;
					tag.other = errminus;
					row.insert( errplus, -1.0 ); // v = eplus - eminus
					row.insert( errminus, 1.0 ); // v - eplus + eminus = 0
					m_objective.insert( errplus, constraint.strength() );
					m_objective.insert( errminus, constraint.strength() );
				}
				else
				{
					Symbol dummy=new Symbol( Symbol.Type.Dummy, m_id_tick++ );
					tag.marker = dummy;
					row.insert( dummy );
				}
				break;
			}
		}

		// Ensure the row as a positive constant.
		if( row.constant() < 0.0 )
			row.reverseSign();

		return row;
	}

	/* Choose the subject for solving for the row.

	This method will choose the best subject for using as the solve
	target for the row. An invalid symbol will be returned if there
	is no valid target.

	The symbols are chosen according to the following precedence:

	1) The first symbol representing an external variable.
	2) A negative slack or error tag variable.

	If a subject cannot be found, an invalid symbol will be returned.

	*/
	Symbol chooseSubject(  Row row,  Tag tag )
	{
        foreach(KeyValuePair<Symbol,double> kv in row.cells())
        {
            if(kv.Key.type()==Symbol.Type.External)
            return kv.Key;
        }
	
		if( tag.marker.type() == Symbol.Type.Slack || tag.marker.type() == Symbol.Type.Error )
		{
			if( row.coefficientFor( tag.marker ) < 0.0 )
				return tag.marker;
		}
		if( tag.other.type() == Symbol.Type.Slack || tag.other.type() == Symbol.Type.Error )
		{
			if( row.coefficientFor( tag.other ) < 0.0 )
				return tag.other;
		}
		return new Symbol();
	}

 	/* Add the row to the tableau using an artificial variable.

	This will return false if the constraint cannot be satisfied.

 	*/
 	bool addWithArtificialVariable(  Row row )
 	{
		// Create and add the artificial variable to the tableau
		Symbol art=new Symbol( Symbol.Type.Slack, m_id_tick++ );
		m_rows[ art ] = new Row( row );
		m_artificial=  new Row( row ) ;

		// Optimize the artificial objective. This is successful
		// only if the artificial objective is optimized to zero.
		optimize( m_artificial );
		bool success = Util.nearZero( m_artificial.constant() );
		m_artificial=null;

		// If the artificial variable is not basic, pivot the row so that
		// it becomes basic. If the row is constant, exit early.
        Row trial;

		if( m_rows.TryGetValue(art,out trial))
		{
			Row rowptr=trial;
			m_rows.Remove( art );
			if( rowptr.cells().Count==0 )
				return success;
			Symbol entering=new Symbol( anyPivotableSymbol( rowptr ) );
			if( entering.type() == Symbol.Type.Invalid )
				return false;  // unsatisfiable (will this ever happen?)
			rowptr.solveFor( art, entering );
			substitute( entering, rowptr );
			m_rows[ entering ] = rowptr;
		}
        
		// Remove the artificial variable from the tableau.
        foreach(KeyValuePair<Symbol,Row> kv in m_rows)
        {
            kv.Value.remove(art);
        }
		m_objective.remove( art );
		return success;
 	}

	/* Substitute the parametric symbol with the given row.

	This method will substitute all instances of the parametric symbol
	in the tableau and the objective function with the given row.

	*/
	void substitute(  Symbol symbol,  Row row )
	{
	
        foreach(KeyValuePair<Symbol,Row> kv in m_rows)
        {
            kv.Value.substitute(symbol,row);
            if(kv.Key.type()!= Symbol.Type.External &&kv.Value.constant() < 0.0) m_infeasible_rows.Add(kv.Key);
        }
	   
		m_objective.substitute( symbol, row );
		if( m_artificial!=null )
			m_artificial.substitute( symbol, row );
	}

	/* Optimize the system for the given objective function.

	This method performs iterations of Phase 2 of the simplex method
	until the objective function reaches a minimum.

	Throws
	------
	InternalSolverError
		The value of the objective function is unbounded.

	*/
	void optimize(  Row objective )
	{
		while( true )
		{
			Symbol entering=new Symbol( getEnteringSymbol( objective ) );
			if( entering.type() == Symbol.Type.Invalid )
				return;
			KeyValuePair<Symbol,Row> it= getLeavingRow( entering );
			if( it.Key == null )
				throw new System.ArgumentException("InternalSolverError: The objective is unbounded.", "SolverImpl"); 
			// pivot the entering symbol into the basis
			Symbol leaving=new Symbol( it.Key );
			Row row = it.Value;
			m_rows.Remove( leaving );
			row.solveFor( leaving, entering );
			substitute( entering, row );
			m_rows[ entering ] = row;
		}
	}

	/* Optimize the system using the dual of the simplex method.

	The current state of the system should be such that the objective
	function is optimal, but not feasible. This method will perform
	an iteration of the dual simplex method to make the solution both
	optimal and feasible.

	Throws
	------
	InternalSolverError
		The system cannot be dual optimized.

	*/
	void dualOptimize()
	{
		while( m_infeasible_rows.Count!=0 )
		{

			Symbol leaving=m_infeasible_rows[m_infeasible_rows.Count-1];
			m_infeasible_rows.RemoveAt(m_infeasible_rows.Count-1);
            Row trial;
            if(m_rows.TryGetValue(leaving,out trial)&&!Util.nearZero(trial.constant())&&trial.constant()<0)
			{
				Symbol entering=new Symbol( getDualEnteringSymbol( trial ) );
				if( entering.type() == Symbol.Type.Invalid )
					throw new System.ArgumentException("InternalSolverError: Dual optimize failed.", "SolverImpl"); 
				// pivot the entering symbol into the basis
				Row row = trial;
				m_rows.Remove( leaving );
				row.solveFor( leaving, entering );
				substitute( entering, row );
				m_rows[ entering ] = row;
			}
		}
	}

	/* Compute the entering variable for a pivot operation.

	This method will return first symbol in the objective function which
	is non-dummy and has a coefficient less than zero. If no symbol meets
	the criteria, it means the objective function is at a minimum, and an
	invalid symbol is returned.

	*/
	Symbol getEnteringSymbol(  Row objective )
	{
        foreach(KeyValuePair<Symbol,double> kv in objective.cells())
        {
          if( kv.Key.type() != Symbol.Type.Dummy && kv.Value < 0.0 )
				 return kv.Key;
        }

		return new Symbol();
	}

	/* Compute the entering symbol for the dual optimize operation.

	This method will return the symbol in the row which has a positive
	coefficient and yields the minimum ratio for its respective symbol
	in the objective function. The provided row *must* be infeasible.
	If no symbol is found which meats the criteria, an invalid symbol
	is returned.

	*/
	Symbol getDualEnteringSymbol(  Row row )
	{
		Symbol entering=new Symbol();
		double ratio = System.Double.MaxValue;
        foreach(KeyValuePair<Symbol,double> kv in row.cells())
        {
          if(kv.Value>0&&kv.Key.type()!=Symbol.Type.Dummy)
          {
              double coeff = m_objective.coefficientFor( kv.Key );
              double r = coeff / kv.Value;
              if( r < ratio )
				{
					ratio = r;
					entering = kv.Key;
				}
          }
        }
		
		return entering;
	}

	/* Get the first Slack or Error symbol in the row.

	If no such symbol is present, and Invalid symbol will be returned.

	*/
	Symbol anyPivotableSymbol(  Row row )
	{
		 foreach(KeyValuePair<Symbol,double> kv in row.cells())
        {
            if(kv.Key.type()== Symbol.Type.Slack|| kv.Key.type()== Symbol.Type.Error)
            return kv.Key;
        }

		
		return new Symbol();
	}

	/* Compute the row which holds the exit symbol for a pivot.

	This method will return an iterator to the row in the row map
	which holds the exit symbol. If no appropriate exit symbol is
	found, the end() iterator will be returned. This indicates that
	the objective function is unbounded.

	*/
	KeyValuePair<Symbol,Row> getLeavingRow(  Symbol entering )
	{
		double ratio = System.Double.MaxValue;
		KeyValuePair<Symbol,Row> found = new KeyValuePair<Symbol,Row>(null,null);
        foreach(KeyValuePair<Symbol,Row> kv in m_rows)
        {
            if(kv.Key.type()!= Symbol.Type.External)
            {
                double temp = kv.Value.coefficientFor( entering );
                if( temp < 0.0 )
				{
					double temp_ratio = -kv.Value.constant() / temp;
					if( temp_ratio < ratio )
					{
						ratio = temp_ratio;
						found = kv;
					}
				}
            }
        }
	
		return found;
	}

	/* Compute the leaving row for a marker variable.

	This method will return an iterator to the row in the row map
	which holds the given marker variable. The row will be chosen
	according to the following precedence:

	1) The row with a restricted basic varible and a negative coefficient
	   for the marker with the smallest ratio of -constant / coefficient.

	2) The row with a restricted basic variable and the smallest ratio
	   of constant / coefficient.

	3) The last unrestricted row which contains the marker.

	If the marker does not exist in any row, the row map end() iterator
	will be returned. This indicates an internal solver error since
	the marker *should* exist somewhere in the tableau.

	*/
	KeyValuePair<Symbol,Row> getMarkerLeavingRow(  Symbol marker )
	{
		double dmax = System.Double.MaxValue;
		double r1 = dmax;
		double r2 = dmax;
		KeyValuePair<Symbol,Row> end = new KeyValuePair<Symbol, Row>(null,null);
		KeyValuePair<Symbol,Row> first = end;
		KeyValuePair<Symbol,Row> second = end;
		KeyValuePair<Symbol,Row> third = end;
        foreach(KeyValuePair<Symbol,Row> kv in m_rows)
        {
            double c = kv.Value.coefficientFor( marker );
            if( c == 0.0 )
				continue;
           if( kv.Key.type() == Symbol.Type.External )
			{
				third = kv;
			}
            else if( c < 0.0 )
			{
				double r = -kv.Value.constant() / c;
				if( r < r1 )
				{
					r1 = r;
					first = kv;
				}
			}
			else
			{
				double r = kv.Value.constant() / c;
				if( r < r2 )
				{
					r2 = r;
					second = kv;
				}
			}
        }
	
		if( first.Key != null )
			return first;
		if( second.Key != null )
			return second;
		return third;
	}

	/* Remove the effects of a constraint on the objective function.

	*/
	void removeConstraintEffects(  Constraint cn,  Tag tag )
	{
		if( tag.marker.type() == Symbol.Type.Error )
			removeMarkerEffects( tag.marker, cn.strength() );
		if( tag.other.type() == Symbol.Type.Error )
			removeMarkerEffects( tag.other, cn.strength() );
	}

	/* Remove the effects of an error marker on the objective function.

	*/
	void removeMarkerEffects(  Symbol marker, double strength )
	{
        Row trial;
        if(m_rows.TryGetValue(marker,out trial))
        {
            m_objective.insert(trial,-strength);
        }
        else
        {
            m_objective.insert(marker,-strength);
        }

	}

	/* Test whether a row is composed of all dummy variables.

	*/
	bool allDummies(  Row row )
	{
	    foreach(KeyValuePair<Symbol,double> kv in row.cells())
        {
        if(kv.Key.type()!=Symbol.Type.Dummy) return false;
        }
	
		return true;
	}

}



} // namespace cassokiwary