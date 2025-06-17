using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ElgamalDemo
{
    class ChuKySoElgamal
    {
        public BigInteger P { get; private set; }
        public BigInteger G { get; private set; }
        public BigInteger Y { get; private set; }

        public BigInteger A { get; private set; }


        public void taoKhoa()
        {
            P = timNTLon(10); // VD: 10 chữ số
            G = TimPhanTuSinh(P);

            // A: khóa bí mật, 1 < A < P-1
            do
            {
                A = RandomBigInteger(P - 1);
            } while (A <= 1 || A >= P - 1);

            //Y = ModPow(G, A, P);


        }

        public BigInteger HashMessage(string message)
        {
            //Băm văn bản
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(message));
                return new BigInteger(hashBytes, isUnsigned: true, isBigEndian: true);
            }
        }

        public (BigInteger r, BigInteger s) ky(string message, BigInteger p, BigInteger g, BigInteger y, BigInteger a, BigInteger k)
        {
            BigInteger H = HashMessage(message);
            BigInteger kInv = timNgichDao(k, p - 1);
            BigInteger r = ModPow(g, k, p);
            BigInteger s = kInv * (H - a * r) % (p - 1);
            return (r, s);
        }

        public bool Verify(string message, BigInteger p, BigInteger g, BigInteger y, BigInteger r, BigInteger s)
        {
            if (r <= 0 || r >= p) return false;
            if (s < 0 || s >= p - 1) return false;

            BigInteger H = HashMessage(message);
            BigInteger left = ModPow(g, H, p);
            BigInteger right = (ModPow(y, r, p) * ModPow(r, s, p)) % p;

            return left == right;
        }
        public bool CheckP(BigInteger soP)
        {
            return CheckNT(soP);
        }
        public bool CheckG(BigInteger g, BigInteger p)
        {
            if (g <= 1 || g >= p)
                return false;

            BigInteger phi = p - 1;
            var factors = TimUocNguyenTo(phi);

            foreach (var factor in factors)
            {
                if (BigInteger.ModPow(g, phi / factor, p) == 1)
                    return false;
            }

            return true;
        }
        public bool CheckA(BigInteger a, BigInteger p)
        {
            if(a < 1 || a >= p - 1)
            {
                return false;
            }
            return true;
        }

        public BigInteger TaoK(BigInteger p)
        {
            BigInteger K;
            do
            {
                K = RandomBigInteger(p - 1);
            } while (GCD(K, p - 1) != 1);
            return K;
        }
        public bool CheckK(BigInteger k, BigInteger p)
        {
            if (k < 1 || k >= p - 1)
            {
                return false;
            }
            return true;
        }

        public BigInteger RandomBigInteger(BigInteger max)
        {
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                byte[] bytes = max.ToByteArray();
                BigInteger result;

                do
                {
                    rng.GetBytes(bytes);
                    bytes[bytes.Length - 1] &= 0x7F; // đảm bảo số dương
                    result = new BigInteger(bytes);
                } while (result >= max || result < 2);

                return result;
            }
        }
        public BigInteger GCD(BigInteger a, BigInteger b)
        {
            //Ham tim UCLN
            while (b != 0)
            {
                BigInteger temp = b;
                b = a % b;
                a = temp;
            }
            return BigInteger.Abs(a);
        }
        public bool CheckNT(BigInteger n)
        {
            //Kiem tra so nt
            if (n < 2) return false;
            if (n == 2 || n == 3) return true;
            if (n % 2 == 0) return false;

            for (BigInteger i = 3; i * i <= n; i += 2)
            {
                if (n % i == 0) return false;
            }
            return true;
        }
        public BigInteger timNTLon(int length)
        {
            //Tim so nguyen to lon
            Random random = new Random();
            BigInteger number;
            do
            {
                // Sinh số ngẫu nhiên có độ dài length
                StringBuilder sb = new StringBuilder();

                // Chữ số đầu tiên phải khác 0
                sb.Append(random.Next(1, 10));
                for (int i = 1; i < length; i++)
                {
                    sb.Append(random.Next(0, 10));
                }

                number = BigInteger.Parse(sb.ToString());
            }
            while (!CheckNT(number)); // Lặp cho đến khi tìm được số nguyên tố

            return number;
        }

        public List<BigInteger> TimUocNguyenTo(BigInteger n)
        {
            List<BigInteger> uoc = new List<BigInteger>();
            for (BigInteger i = 2; i * i <= n; i++)
            {
                if (n % i == 0)
                {
                    uoc.Add(i);
                    while (n % i == 0)
                        n /= i;
                }
            }
            if (n > 1)
                uoc.Add(n);
            return uoc;
        }

        public BigInteger TimPhanTuSinh(BigInteger p)
        {
            BigInteger phi = p - 1;
            List<BigInteger> uoc = TimUocNguyenTo(phi);

            for (BigInteger g = p / 2; g >= 2; g--)
            {
                bool isGenerator = true;
                foreach (var q in uoc)
                {
                    if (BigInteger.ModPow(g, phi / q, p) == 1)
                    {
                        isGenerator = false;
                        break;
                    }
                }

                if (isGenerator)
                    return g;
            }

            throw new Exception("Không tìm thấy phần tử sinh");
        }

        public BigInteger ModPow(BigInteger a, BigInteger b, BigInteger n)
        {
            //a^b mod n
            return BigInteger.ModPow(a, b, n);
        }
        public BigInteger timNgichDao(BigInteger a, BigInteger m)
        {
            BigInteger m0 = m, t, q;
            BigInteger x0 = 0, x1 = 1;

            if (m == 1)
                return 0;

            while (a > 1)
            {
                q = a / m;
                t = m;

                m = a % m;
                a = t;

                t = x0;
                x0 = x1 - q * x0;
                x1 = t;
            }

            if (x1 < 0)
                x1 += m0;

            return x1;
        }
    }
}
