using System;
using System.Collections.Generic;
using System.Linq;

public class BigNumber
{
    private byte[] digits;
    private bool isNegative;

    // Constructori
    public BigNumber() {}

    public BigNumber(string s)
    {
        if (s[0] == '-')
        {
            isNegative = true;
            s = s.Remove(0, 1);
        }
        else isNegative = false;

        digits = new byte[(s.Length+1)/2];
        for (int i = s.Length - 1, j = digits.Length - 1; i >= 0; i -= 2, j -= 1)
        {
            if (i == 0)
                digits[j] = (byte)(s[i] - '0');
            else
            {
                digits[j] = (byte)(s[i - 1] - '0');
                digits[j] *= 10;
                digits[j] += (byte)(s[i] - '0');
            }
        }
    }

    public BigNumber(BigNumber x)
    {
        isNegative = x.isNegative;
        digits = (byte[])x.digits.Clone();
    }

    public BigNumber(int x)
    {
        if (x < 0)
        {
            isNegative = true;
            x = -x;
        }
        else isNegative = false;

        digits = new byte[(x.ToString().Length + 1) / 2];
        for (int i = x.ToString().Length - 1, j = digits.Length - 1; i >= 0; i -= 2, j -= 1)
        {
            digits[j] = (byte)(x%100);
            x /= 100;
        }
    }

    public BigNumber(byte[] x)
    {
        isNegative = false;
        digits = (byte[])x.Clone();
    }

    public BigNumber(byte[] x, bool neg)
    {
        isNegative = neg;
        digits = (byte[])x.Clone();
    }

    public BigNumber(List<byte> x)
    {
        isNegative = false;
        digits = x.ToArray();
    }

    public BigNumber(List<byte> x, bool neg)
    {
        isNegative = neg;
        digits = x.ToArray();
    }

    // Misc
    public int Length()
    {
        return digits.Length;
    }

    // Operatori aritmetici
    public static BigNumber operator +(BigNumber a, BigNumber b)
    {
        if (a == 0) return b;
        if (b == 0) return a;

        bool negativeResult = false;
        if (!a.isNegative && !b.isNegative) negativeResult = false;
        else
        {
            if (a.isNegative && b.isNegative)
            {
                negativeResult = true;
                a = BigNumber.Abs(a);
                b = BigNumber.Abs(b);

            }
            else
            {
                return BigNumber.Max(a, b) - BigNumber.Abs(BigNumber.Min(a, b));
            }
        }


        byte[] tempArray = new byte[Math.Max(a.Length(), b.Length()) + 1];

        int i, j, temp, carry = 0;
        for(i=a.Length()-1, j=b.Length()-1; i>=0 && j >=0; i--, j--)
        {
            temp = a.digits[i] + b.digits[j] + carry;
            tempArray[Math.Max(i, j) + 1] = (byte)(temp % 100);
            carry = temp >= 100 ? 1 : 0;
        }

        if (i < 0 && j < 0 && carry == 1) tempArray[0] = (byte)1;

        if (i >= 0)
            for (; i >= 0; i--) 
            {
                temp = a.digits[i] + carry;
                tempArray[i + 1] = (byte)(temp % 100);
                carry = temp >= 100 ? 1 : 0;
            }
        if (j >= 0)
            for (; j >= 0; j--)
            {
                temp = b.digits[j] + carry;
                tempArray[j + 1] = (byte)(temp % 100);
                carry = temp >= 100 ? 1 : 0;
            }

        if (tempArray[0] == (byte)0)
            tempArray = tempArray.Skip(1).ToArray();
                
        return new BigNumber(tempArray, negativeResult);
    }

    public static BigNumber operator ++(BigNumber a)
    {
        return a + 1;
    }

    public static BigNumber operator -(BigNumber a, BigNumber b)
    {

        // ca sa evit crearea unui -0
        if (a == 0 && b == 0) return new BigNumber("0");

        if (a == 0) return -b;
        if (b == 0) return a;


        if(!(!a.isNegative && !b.isNegative))
        {

            if(a.isNegative && b.isNegative)
            {

                if (BigNumber.Abs(a) > BigNumber.Abs(b)) return -(-a - (-b));
                else return -b - (-a);
            }
            else
            {
                return a + (-b);
            }
        }

        bool negativeResult = false;
        if (a < b)
        {
            negativeResult = true;
            BigNumber c = new BigNumber(a);
            a = b;
            b = c;
        }



        BigNumber tempA = new BigNumber(a);

        byte[] tempArray = new byte[tempA.Length()];

        int i, j, temp;

        for (i = tempA.Length() - 1, j = b.Length() - 1; i >= 0 && j >= 0; i--, j--)
        {
            temp = tempA.digits[i] - b.digits[j];
            if (temp < 0)
            {
                temp += 100;

                if (tempA.digits[i - 1] > 0) tempA.digits[i - 1]--;
                else
                {
                    int k;
                    for (k = i - 1; k >= 0 && tempA.digits[k] == 0; k--) tempA.digits[k] = 99;

                    tempA.digits[k]--;
                }
            }
            tempArray[i] = (byte)temp;
        }

        if(i>=0 && j < 0)
        {
            for(; i>=0; i--) tempArray[i] = tempA.digits[i];
        }

        for (i = 0; tempArray[i] == 0 && i < tempArray.Length - 1; i++) ;

        if (tempArray[0] == (byte)0)
            tempArray = tempArray.Skip(i).ToArray();

        return new BigNumber(tempArray, negativeResult); 
    }

    public static BigNumber operator --(BigNumber a)
    {
        return a - 1;
    }

    public static BigNumber operator -(BigNumber a)
    {
        BigNumber temp = new BigNumber(a);
        temp.isNegative = !temp.isNegative;
        return temp;
    }

    public static BigNumber operator *(BigNumber a, BigNumber b)
    {
        if (a == 0 || b == 0) return new BigNumber(0);
        if (a == 1) return b;
        if (b == 1) return a;

        bool negativeResult = a.isNegative != b.isNegative ? true : false;

        a = BigNumber.Abs(a);
        b = BigNumber.Abs(b);

        byte[] result = new byte[a.Length()+b.Length()];
        byte[] temp = new byte[a.Length()+1];

        for (int i = b.Length() - 1; i >= 0; i--)
        {
            int j, k, x = 0;
            temp = new byte[a.Length() + 1];

            for (j = a.Length() - 1, k = temp.Length - 1; j >= 0; j--, k--)
            {
                x += a.digits[j] * b.digits[i];
                temp[k] = (byte)(x % 100);
                x /= 100;
            }

            for (; x > 0; k--)
            {
                temp[k] = (byte)(x % 100);
                x /= 100;
            }

            int carry = 0;
            for (j = result.Length - 1 - (b.Length() - 1 - i), k = temp.Length - 1; k >= 0; j--, k--)
            {
                result[j] += (byte)(temp[k] + carry);
                carry = result[j] / 100;
                result[j] %= 100;
            }
        }

        if (result[0]==0) result = result.Skip(1).ToArray();

        return new BigNumber(result, negativeResult);
    }

    public static BigNumber operator /(BigNumber a, BigNumber b)
    {
        bool negativeResult = a.isNegative != b.isNegative ? true : false;

        a = BigNumber.Abs(a);
        b = BigNumber.Abs(b);

        if (b == 0) throw new DivideByZeroException();
        if (a == 0) return new BigNumber("0");
        if (a < b) return new BigNumber("0");
        if (a == b) return new BigNumber("1");

        List<byte> tempList = new List<byte>();

        List<byte> result = new List<byte>();

        BigNumber temp = 0;

        int pos = 0;

        while(pos < a.Length())
        {
            for (; temp < b && pos < a.Length(); pos++)
            {
                if(!(tempList.Count == 0 && a.digits[pos] == 0))
                    tempList.Add(a.digits[pos]);

                temp = new BigNumber(tempList);

                if (temp < b && result.Count != 0) result.Add((byte)0);
            }

            if (temp >= b)
            {
                int j = 0;
                for (; temp >= b; j++) temp -= b;
                result.Add((byte)j);

                if (temp == 0) tempList = new List<byte>();
                else tempList = temp.digits.ToList();
            }
        }

        return new BigNumber(result, negativeResult);
    }

    public static BigNumber operator ^(BigNumber a, BigNumber b)
    {
        if (a == 0 && b == 0) throw new Exception("Tried computing 0^0");

        if (b < 0)
        {
            if (a == 1) return new BigNumber(1);
            if (a == -1)
            {
                if(b % 2 == 0) return new BigNumber(1);
                return new BigNumber(-1);
            }

            return new BigNumber(0);
        }

        if (a == 0) return new BigNumber(0);
        if (b == 0) return new BigNumber(1);

        BigNumber result = new BigNumber(a);

        for(BigNumber i = 1; i < b; i++)
        {
            result *= a;
        }

        return result;
    }

    public static BigNumber Sqrt(BigNumber a)
    {
        if (a < 0) throw new Exception("Tried computing the square root of a negative number");
        if (a == 0) return new BigNumber(0);
        if (a == 1) return new BigNumber(1);

        // Algoritmul babilonian

        BigNumber guess = a / 2;
        BigNumber test = a / guess;


        while (test != guess && (BigNumber.Max(test, guess) - BigNumber.Min(test, guess) != 1))
        {
            test = a / guess;
            guess = (test + guess) / 2;
        }

        if (guess * guess > a) guess--;

        return guess;
    }

    // Functii matematice
    public static BigNumber Max(BigNumber a, BigNumber b)
    {
        if (a > b) return a;
        return b;
    }

    public static BigNumber Min(BigNumber a, BigNumber b)
    {
        if (a < b) return a;
        return b;
    }

    public static BigNumber Abs(BigNumber a)
    {
        if (a.isNegative) return -a;
        else return a;
    }

    public static BigNumber operator %(BigNumber a, BigNumber b)
    {
        bool isFirstNegative = a.isNegative;
        a = BigNumber.Abs(a);
        b = BigNumber.Abs(b);

        if (b == 0) throw new DivideByZeroException();

        if (a < b) return a;
        if (a == b) return 0;

        List<byte> tempList = new List<byte>();

        BigNumber temp = 0;

        int pos = 0;

        while (pos < a.Length())
        {
            for (; temp < b && pos < a.Length(); pos++)
            {
                tempList.Add(a.digits[pos]);
                temp = new BigNumber(tempList);
            }

            if (temp >= b)
            {
                int j = 0;
                for (; temp >= b; j++) temp -= b;

                if (temp == 0) tempList = new List<byte>();
                else tempList = temp.digits.ToList();
            }
        }

        temp.isNegative = isFirstNegative;
        return temp;
    }

    // Operatori relationali
    public static bool operator ==(BigNumber a, BigNumber b)
    {
        // -0 este egal cu +0
        if (a.digits[0] == 0 && b.digits[0] == 0) return true;
        
        if(a.isNegative != b.isNegative) return false;

        if (a.Length() != b.Length()) return false;

        for(int i=0; i<a.Length(); i++)
            if (a.digits[i] != b.digits[i]) return false;

        return true;
    }

    public static bool operator !=(BigNumber a, BigNumber b)
    {

        if (a.digits[0] == 0 && b.digits[0] == 0) return false;

        if (a.isNegative != b.isNegative) return true;


        if (a.Length() != b.Length()) return true;

        for (int i = 0; i < a.Length(); i++)
            if (a.digits[i] != b.digits[i]) return true;

        return false;
    }

    public static bool operator <(BigNumber a, BigNumber b)
    {
        if (a.digits[0] == 0 && b.digits[0] == 0) return false;

        if (a.isNegative && !b.isNegative) return true;
        if (!a.isNegative && b.isNegative) return false;
        
        if(!a.isNegative && !b.isNegative)
        {
            if (a.Length() < b.Length()) return true;
            if (a.Length() > b.Length()) return false;


            for (int i = 0; i < a.Length(); i++)
                if (a.digits[i] != b.digits[i])
                    return a.digits[i] < b.digits[i];

            return false;
        }
        else
        {
            if (a.Length() < b.Length()) return false;
            if (a.Length() > b.Length()) return true;


            for (int i = 0; i < a.Length(); i++)
                if (a.digits[i] != b.digits[i])
                    return !(a.digits[i] < b.digits[i]);

            return false;
        }


    }

    public static bool operator >(BigNumber a, BigNumber b)
    {
        if (a.digits[0] == 0 && b.digits[0] == 0) return false;

        if (a.isNegative && !b.isNegative) return false;
        if (!a.isNegative && b.isNegative) return true;

        if(!a.isNegative && !b.isNegative)
        {
            if (a.Length() > b.Length()) return true;
            if (a.Length() < b.Length()) return false;


            for (int i = 0; i < a.Length(); i++)
                if (a.digits[i] != b.digits[i])
                    return a.digits[i] > b.digits[i];

            return false;
        }
        else
        {
            if (a.Length() > b.Length()) return false;
            if (a.Length() < b.Length()) return true;


            for (int i = 0; i < a.Length(); i++)
                if (a.digits[i] != b.digits[i])
                    return !(a.digits[i] > b.digits[i]);

            return false;
        }


    }

    public static bool operator >=(BigNumber a, BigNumber b)
    {
        return (a > b) || (a == b);
    }

    public static bool operator <=(BigNumber a, BigNumber b)
    {
        return (a < b) || (a == b);
    }

    // Conversii intre diferite tipuri de date

    public static implicit operator BigNumber(int x)
    {
        return new BigNumber(x);
    }

    public static implicit operator BigNumber(string str)
    {
        return new BigNumber(str);
    }

    public static implicit operator String(BigNumber bn)
    {
        if (bn.digits == null) return "";


        string str = "";
        if (bn.isNegative) str += '-';
        str += bn.digits[0].ToString();

        for (int i=1; i<bn.Length(); i++)
        {
            if (bn.digits[i] < 10) str += "0" + bn.digits[i].ToString();
            else str += bn.digits[i].ToString();
        }

        return str;
    }
}


namespace Operatii_cu_numere_mari
{
    internal class Program
    {
        static void Main()
        {
            // Niste exemple de operatii

            BigNumber a = new BigNumber("-548946546");
            BigNumber b = new BigNumber("28956465445");
            BigNumber c = new BigNumber(65536);


            Console.WriteLine(a + b);
            Console.WriteLine(BigNumber.Sqrt(b));
            Console.WriteLine(a * b);
            Console.WriteLine(b % a);
            Console.WriteLine(a ^ 3);
            Console.WriteLine(a / 32);
            Console.WriteLine(b - a);
            Console.WriteLine(BigNumber.Sqrt(c));


            Console.ReadKey();
        }
    }
}
