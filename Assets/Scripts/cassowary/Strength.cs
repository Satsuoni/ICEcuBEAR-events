using System;

namespace cassowary
{
    public enum Rel
    {
        Required,
        Strong,
        Medium,
        Weak
    }
    public static class Strength
{
public static  double create( double a, double b, double c, double w = 1.0 )
{
	double result = 0.0;
	result += Math.Max( 0.0, Math.Min( 1000.0, a * w ) ) * 1000000.0;
	result += Math.Max( 0.0, Math.Min( 1000.0, b * w ) ) * 1000.0;
	result += Math.Max( 0.0, Math.Min( 1000.0, c * w ) );
	return result;
}
public const double reqq=1000*1000000+1000*1000.0+1000;
public static double required = create( 1000.0, 1000.0, 1000.0 );
public static  double strong = create( 1.0, 0.0, 0.0 );
public static  double medium = create( 0.0, 1.0, 0.0 );

public static  double weak = create( 0.0, 0.0, 1.0 );
        
public static double clip( double value )
{
	return Math.Max( 0.0, Math.Min( required, value ) );
}
        public static double fromEnum(Rel num)
        {
            switch (num)
            {
                case Rel.Required: return required;
                case Rel.Strong: return strong;
                case Rel.Medium: return medium;
                case Rel.Weak: return weak;

            }

            return 0;
        }

    }
   

    

} // namespace kiwi