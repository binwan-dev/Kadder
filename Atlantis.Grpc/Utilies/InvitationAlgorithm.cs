using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Followme.AspNet.Core.FastCommon.Utilities
{
    public class InvitationAlgorithm: SerialAlgorithm
    {
        public readonly static InvitationAlgorithm Instance = new InvitationAlgorithm();

        private InvitationAlgorithm() :
            base(new char[] { 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', '1', '2', '3' },
           new char[] { '9', '0', '4', 'A', 'Z', '5', 'Y', '6', '7', 'B', 'C', '8' }, 6)
        {

        }
    }
    

    public class SerialAlgorithm
    {
        /**自定义进制(0,1没有加入,容易与o,l混淆)*/
        private readonly char[] r ;
        /**自动补全组(不能与自定义进制有重复)*/
        private readonly char[] b ;
        /**进制长度*/
        private readonly int l;

        private readonly int bl ;
        /**序列最小长度*/
        private readonly int s = 6;

        private readonly Random random = new Random(DateTime.Now.Millisecond);

        public SerialAlgorithm(char[] r, char[] b,int serialLength)
        {
            this.r = r;
            this.b = b;
            this.s = serialLength;

            l = r.Length;
            bl = b.Length;
        }

        /**
          * 根据ID生成六位随机码
          * @param num ID
          * @return 随机码
          */
        public String toSerialNumber(long num)
        {
            char[] buf = new char[32];
            int charPos = 32;

            while ((num / l) > 0)
            {
                buf[--charPos] = r[(int)(num % l)];
                num /= l;
            }
            buf[--charPos] = r[(int)(num % l)];
            String str = new String(buf, charPos, (32 - charPos));
            //不够长度的自动随机补全
            if (str.Length < s)
            {
                StringBuilder sb = new StringBuilder();
                Random rnd = new Random();
                for (int i = 0; i < s - str.Length; i++)
                {
                    sb.Append(b[rnd.Next(bl)]);
                }
                str += sb.ToString();
            }
            return str.ToLower();
        }

        /// <summary>
        /// 生成一个非重复的随机序列。
        /// </summary>
        /// <param name="low">序列最小值。</param>
        /// <param name="high">序列最大值。</param>
        /// <returns>序列。</returns>
        private int[] BuildRandomSequence4(int low, int high)
        {
            int x = 0, tmp = 0;
            if (low > high)
            {
                tmp = low;
                low = high;
                high = tmp;
            }
            int[] array = new int[high - low + 1];
            for (int i = low; i <= high; i++)
            {
                array[i - low] = i;
            }
            for (int i = array.Length - 1; i > 0; i--)
            {
                x = random.Next(0, i + 1);
                tmp = array[i];
                array[i] = array[x];
                array[x] = tmp;
            }
            return array;
        }
    }
}
