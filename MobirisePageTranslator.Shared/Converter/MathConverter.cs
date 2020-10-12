using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.UI.Xaml.Data;

namespace MobirisePageTranslator.Shared.Converter
{
    /// <see cref="IValueConverter"/>
    /// <summary>
    /// This is a converter class which provides math operations on bound value.
    /// </summary>
    public class MathConverter : IValueConverter
    {
        private static readonly char[] myOperators = {'+', '-', '*', '/', '%', '(', ')'};
        private static readonly List<string> myGroupSigns = new List<string> {"(", ")"};
        private static readonly List<string> myMathOperators = new List<string> {"+", "-", "*", "/", "%"};

        /// <see cref="IValueConverter.Convert"/>
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source and can be used as @VALUE in the parameter statement.</param>
        /// <param name="targetType">Not used.</param>
        /// <param name="parameter">The math statement which should be executed on the value.</param>
        /// <param name="culture">Not used.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <exception cref="InvalidOperationException">If the given math statement is not executable.</exception>
        /// <exception cref="InvalidCastException">If the given value or statement is not numeric parseable.</exception>
        /// <exception cref="FormatException">If the given statement is not in correct format.</exception>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var mathEquation = PrepareInput( value, parameter );
            var numbers = mathEquation.Split( myOperators ).Where( s => s != string.Empty ).Select( double.Parse ).ToList();

            EvaluateMathString( ref mathEquation, ref numbers, 0 );

            return numbers[ 0 ];
        }

        /// <see cref="IValueConverter.ConvertBack"/>
        /// <summary>
        /// Convert back is not supported.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <exception cref="NotSupportedException">Always thrown.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }

        private static string PrepareInput( object value, object parameter )
        {
            string mathEquation = parameter.ToString();
            mathEquation = mathEquation.Replace( " ", string.Empty );
            mathEquation = mathEquation.Replace( "@VALUE", value.ToString() );
            return mathEquation;
        }

        private static void EvaluateMathString( ref string mathEquation, ref List<double> numbers, int index )
        {
            string statement = GetNextStatement( mathEquation );

            while( statement != string.Empty )
            {
                mathEquation = mathEquation.Remove( 0, statement.Length );

                if( myGroupSigns.Contains( statement ) )
                {
                    switch( statement )
                    {
                        case "(":
                            EvaluateMathString( ref mathEquation, ref numbers, index );
                            break;
                        case ")":
                            return;
                        default:
                            break;
                    }
                }

                if( myMathOperators.Contains( statement ) )
                {
                    ParseStatement( ref mathEquation, ref numbers, index, statement );
                }

                statement = GetNextStatement( mathEquation );
            }
        }

        private static void ParseStatement( ref string mathEquation, ref List<double> numbers, int index, string statement )
        {
            string nextStatement = GetNextStatement( mathEquation );
            if( nextStatement == "(" )
            {
                EvaluateMathString( ref mathEquation, ref numbers, index + 1 );
            }

            if( numbers.Count > (index + 1) &&
                (Math.Abs( double.Parse( nextStatement ) - numbers[ index + 1 ] ) < 0.0001 || nextStatement == "(") )
            {
                switch( statement )
                {
                    case "+":
                        numbers[ index ] = numbers[ index ] + numbers[ index + 1 ];
                        break;
                    case "-":
                        numbers[ index ] = numbers[ index ] - numbers[ index + 1 ];
                        break;
                    case "*":
                        numbers[ index ] = numbers[ index ] * numbers[ index + 1 ];
                        break;
                    case "/":
                        numbers[ index ] = numbers[ index ] / numbers[ index + 1 ];
                        break;
                    case "%":
                        numbers[ index ] = numbers[ index ] % numbers[ index + 1 ];
                        break;
                }
                numbers.RemoveAt( index + 1 );
            }
            else
            {
                throw new FormatException( "Next token is not the expected number" );
            }
        }

        private static string GetNextStatement( string mathEquation )
        {
            string result = string.Empty;

            if( mathEquation != string.Empty )
            {
                foreach( char c in mathEquation )
                {
                    if( myOperators.Contains( c ) )
                    {
                        result = (string.IsNullOrEmpty( result ) ? c.ToString( CultureInfo.InvariantCulture ) : result);
                        break;
                    }
                    result += c;
                }
            }

            return result;
        }
    }
}