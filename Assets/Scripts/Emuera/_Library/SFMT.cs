/*
このファイルはSFMTアルゴリズムによって擬似乱数を作成するためのクラスライブラリです。
このファイルはRei HOBARAさんが
http://www.rei.to/random.html
において公開しているC#向けのSFMTライブラリを改変したものです。

さらに大元のSFMTアルゴリズムについては
http://www.math.sci.hiroshima-u.ac.jp/~m-mat/MT/SFMT/
を参照してください。

2009/7/2 MinorShift
*/

/*
 * Copyright (C) Rei HOBARA 2007
 * 
 * Name:
 *     SFMT.cs
 * Class:
 *     Rei.Random.SFMT
 *     Rei.Random.MTPeriodType
 * Purpose:
 *     A random number generator using SIMD-oriented Fast Mersenne Twister(SFMT).
 * Remark:
 *     This code is C# implementation of SFMT.
 *     SFMT was introduced by Mutsuo Saito and Makoto Matsumoto.
 *     See http://www.math.sci.hiroshima-u.ac.jp/~m-mat/MT/SFMT/index.html for detail of SFMT.
 * History:
 *     2007/10/6 initial release.
 * 
 */
 
#define MT19937
using System;
namespace MinorShift._Library{

    /// <summary>
    /// SFMTの擬似乱数ジェネレータークラス。
    /// </summary>
    public sealed class MTRandom {

        /// <summary>
        /// 現在時刻を種とした、(2^19937-1)周期のSFMT擬似乱数ジェネレーターを初期化します。
        /// </summary>
        public MTRandom() : this(Environment.TickCount) { }

        /// <summary>
        /// seedを種とした、(2^MEXP-1)周期の擬似乱数ジェネレーターを初期化します。
        /// </summary>
		public MTRandom(Int64 seed)
        {
			unchecked
			{
				init_gen_rand((UInt32)seed);
			}
        }
        
        
        //maxが2^nでない大きい値であると値が偏る。
        public Int64 NextInt64(Int64 max)
        {
			if(max <= 0)
				throw new ArgumentOutOfRangeException();
			return (Int64)(NextUInt64() % (UInt64)max);
		}
		
        public Int64 NextInt64()
        {
			unchecked{return (Int64)NextUInt64();}
		}
		
        public UInt64 NextUInt64()
        {
			UInt64 ret = NextUInt32();
			ret = (ret << 32) +  NextUInt32();
			return ret;
		}
        
        /// <summary>
		/// [0,1) 範囲で乱数生成 ←0は含む,1は含まないの意味
		/// </summary>
		/// <returns></returns>
		public double NextDouble()
		{
			return (double)NextUInt32() * ((double)1.0/4294967296.0); 
			/* divided by 2^32 */
		}
        
        public void SetRand(Int64[] array)
        {
			if((array == null)|| (array.Length != (N32 + 1)))
				throw new ArgumentOutOfRangeException();
			
			for(int i = 0;i<N32;i++)
				sfmt[i] = (UInt32)array[i];
			idx = (int)array[N32];
		}

		public void GetRand(Int64[] array)
		{
			if ((array == null) || (array.Length != (N32 + 1)))
				throw new ArgumentOutOfRangeException();
			for(int i = 0;i<N32;i++)
				array[i] = sfmt[i];
			array[N32] = idx;
		}
        
        
#region private/protected
		#if MT607
			private const int MEXP = 607;
			private const int POS1 = 2;
			private const int SL1 = 15;
			private const int SL2 = 3;
			private const int SR1 = 13;
			private const int SR2 = 3;
			private const UInt32 MSK1 = 0xfdff37ffU;
			private const UInt32 MSK2 = 0xef7f3f7dU;
			private const UInt32 MSK3 = 0xff777b7dU;
			private const UInt32 MSK4 = 0x7ff7fb2fU;
			private const UInt32 PARITY1 = 0x00000001U;
			private const UInt32 PARITY2 = 0x00000000U;
			private const UInt32 PARITY3 = 0x00000000U;
			private const UInt32 PARITY4 = 0x5986f054U;
		#elif MT1279
			private const int MEXP = 1279;
			private const int POS1 = 7;
			private const int SL1 = 14;
			private const int SL2 = 3;
			private const int SR1 = 5;
			private const int SR2 = 1;
			private const UInt32 MSK1 = 0xf7fefffdU;
			private const UInt32 MSK2 = 0x7fefcfffU;
			private const UInt32 MSK3 = 0xaff3ef3fU;
			private const UInt32 MSK4 = 0xb5ffff7fU;
			private const UInt32 PARITY1 = 0x00000001U;
			private const UInt32 PARITY2 = 0x00000000U;
			private const UInt32 PARITY3 = 0x00000000U;
			private const UInt32 PARITY4 = 0x20000000U;
		#elif MT2281
			private const int MEXP = 2281;
			private const int POS1 = 12;
			private const int SL1 = 19;
			private const int SL2 = 1;
			private const int SR1 = 5;
			private const int SR2 = 1;
			private const UInt32 MSK1 = 0xbff7ffbfU;
			private const UInt32 MSK2 = 0xfdfffffeU;
			private const UInt32 MSK3 = 0xf7ffef7fU;
			private const UInt32 MSK4 = 0xf2f7cbbfU;
			private const UInt32 PARITY1 = 0x00000001U;
			private const UInt32 PARITY2 = 0x00000000U;
			private const UInt32 PARITY3 = 0x00000000U;
			private const UInt32 PARITY4 = 0x41dfa600U;
		#elif MT4253
			private const int MEXP = 4253;
			private const int POS1 = 17;
			private const int SL1 = 20;
			private const int SL2 = 1;
			private const int SR1 = 7;
			private const int SR2 = 1;
			private const UInt32 MSK1 = 0x9f7bffffU;
			private const UInt32 MSK2 = 0x9fffff5fU;
			private const UInt32 MSK3 = 0x3efffffbU;
			private const UInt32 MSK4 = 0xfffff7bbU;
			private const UInt32 PARITY1 = 0xa8000001U;
			private const UInt32 PARITY2 = 0xaf5390a3U;
			private const UInt32 PARITY3 = 0xb740b3f8U;
			private const UInt32 PARITY4 = 0x6c11486dU;
		#elif MT11213
			private const int MEXP = 11213;
			private const int POS1 = 68;
			private const int SL1 = 14;
			private const int SL2 = 3;
			private const int SR1 = 7;
			private const int SR2 = 3;
			private const UInt32 MSK1 = 0xeffff7fbU;
			private const UInt32 MSK2 = 0xffffffefU;
			private const UInt32 MSK3 = 0xdfdfbfffU;
			private const UInt32 MSK4 = 0x7fffdbfdU;
			private const UInt32 PARITY1 = 0x00000001U;
			private const UInt32 PARITY2 = 0x00000000U;
			private const UInt32 PARITY3 = 0xe8148000U;
			private const UInt32 PARITY4 = 0xd0c7afa3U;
		#elif MT19937
			private const int MEXP = 19937;
			private const int POS1 = 122;
			private const int SL1 = 18;
			private const int SL2 = 1;
			private const int SR1 = 11;
			private const int SR2 = 1;
			private const UInt32 MSK1 = 0xdfffffefU;
			private const UInt32 MSK2 = 0xddfecb7fU;
			private const UInt32 MSK3 = 0xbffaffffU;
			private const UInt32 MSK4 = 0xbffffff6U;
			private const UInt32 PARITY1 = 0x00000001U;
			private const UInt32 PARITY2 = 0x00000000U;
			private const UInt32 PARITY3 = 0x00000000U;
			private const UInt32 PARITY4 = 0x13c9e684U;
			//private const UInt32 PARITY4 = 0x20000000U;
		#elif MT44497
			private const int MEXP = 44497;
			private const int POS1 = 330;
			private const int SL1 = 5;
			private const int SL2 = 3;
			private const int SR1 = 9;
			private const int SR2 = 3;
			private const UInt32 MSK1 = 0xeffffffbU;
			private const UInt32 MSK2 = 0xdfbebfffU;
			private const UInt32 MSK3 = 0xbfbf7befU;
			private const UInt32 MSK4 = 0x9ffd7bffU;
			private const UInt32 PARITY1 = 0x00000001U;
			private const UInt32 PARITY2 = 0x00000000U;
			private const UInt32 PARITY3 = 0xa3ac4000U;
			private const UInt32 PARITY4 = 0xecc1327aU;
		#elif MT86243
			private const int MEXP = 86243;
			private const int POS1 = 366;
			private const int SL1 = 6;
			private const int SL2 = 7;
			private const int SR1 = 19;
			private const int SR2 = 1;
			private const UInt32 MSK1 = 0xfdbffbffU;
			private const UInt32 MSK2 = 0xbff7ff3fU;
			private const UInt32 MSK3 = 0xfd77efffU;
			private const UInt32 MSK4 = 0xbf9ff3ffU;
			private const UInt32 PARITY1 = 0x00000001U;
			private const UInt32 PARITY2 = 0x00000000U;
			private const UInt32 PARITY3 = 0x00000000U;
			private const UInt32 PARITY4 = 0xe9528d85U;
		#elif MT132049
			private const int MEXP = 132049;
			private const int POS1 = 110;
			private const int SL1 = 19;
			private const int SL2 = 1;
			private const int SR1 = 21;
			private const int SR2 = 1;
			private const UInt32 MSK1 = 0xffffbb5fU;
			private const UInt32 MSK2 = 0xfb6ebf95U;
			private const UInt32 MSK3 = 0xfffefffaU;
			private const UInt32 MSK4 = 0xcff77fffU;
			private const UInt32 PARITY1 = 0x00000001U;
			private const UInt32 PARITY2 = 0x00000000U;
			private const UInt32 PARITY3 = 0xcb520000U;
			private const UInt32 PARITY4 = 0xc7e91c7dU;
		#elif MT216091
			private const int MEXP = 216091;
			private const int POS1 = 627;
			private const int SL1 = 11;
			private const int SL2 = 3;
			private const int SR1 = 10;
			private const int SR2 = 1;
			private const UInt32 MSK1 = 0xbff7bff7U;
			private const UInt32 MSK2 = 0xbfffffffU;
			private const UInt32 MSK3 = 0xbffffa7fU;
			private const UInt32 MSK4 = 0xffddfbfbU;
			private const UInt32 PARITY1 = 0xf8000001U;
			private const UInt32 PARITY2 = 0x89e80709U;
			private const UInt32 PARITY3 = 0x3bd2b64bU;
			private const UInt32 PARITY4 = 0x0c64b1e4U;
		}
		#endif

        private const int N = MEXP / 128 + 1;
        private const int N32 = N * 4;
        private const int SL2_x8 = SL2 * 8;
        private const int SR2_x8 = SR2 * 8;
        private const int SL2_ix8 = 64 - SL2 * 8;
        private const int SR2_ix8 = 64 - SR2 * 8;
        
        /// <summary>
        /// 内部状態ベクトル。
        /// </summary>
        private UInt32[] sfmt;
        /// <summary>
        /// 内部状態ベクトルのうち、次に乱数として使用するインデックス。
        /// </summary>
		private int idx;
        
        /// <summary>
        /// 符号なし32bitの擬似乱数を取得します。
        /// </summary>
		private UInt32 NextUInt32()
		{
            if (idx >= N32) {
                gen_rand_all();
                idx = 0;
            }
            return sfmt[idx++];
        }

        /// <summary>
        /// ジェネレーターを初期化します。
        /// </summary>
        /// <param name="seed"></param>
		private void init_gen_rand(UInt32 seed)
		{
            int i;
            //内部状態配列確保
            sfmt = new UInt32[N32];
            //内部状態配列初期化
            sfmt[0] = seed;
            for (i = 1; i < N32; i++)
                sfmt[i] = (UInt32)(1812433253 * (sfmt[i - 1] ^ (sfmt[i - 1] >> 30)) + i);
            //確認
            period_certification();
            //初期位置設定
            idx = N32;
        }

        /// <summary>
        /// 内部状態ベクトルが適切か確認し、必要であれば調節します。
        /// </summary>
		private void period_certification()
		{
            UInt32[] PARITY = new UInt32[] { PARITY1, PARITY2, PARITY3, PARITY4 };
            UInt32 inner = 0;
            int i, j;
            UInt32 work;

            for (i = 0; i < 4; i++) inner ^= sfmt[i] & PARITY[i];
            for (i = 16; i > 0; i >>= 1) inner ^= inner >> i;
            inner &= 1;
            // check OK
            if (inner == 1) return;
            // check NG, and modification
            for (i = 0; i < 4; i++) {
                work = 1;
                for (j = 0; j < 32; j++) {
                    if ((work & PARITY[i]) != 0) {
                        sfmt[i] ^= work;
                        return;
                    }
                    work = work << 1;
                }
            }
        }

        /// <summary>
        /// 内部状態ベクトルを更新します。
        /// </summary>
		private void gen_rand_all()
		{
#if MT19937
			gen_rand_all_19937();
#else
			int a, b, c, d;
			UInt64 xh, xl, yh, yl;

			a = 0;
			b = POS1 * 4;
			c = (N - 2) * 4;
			d = (N - 1) * 4;
			do
			{
				xh = ((UInt64)sfmt[a + 3] << 32) | sfmt[a + 2];
				xl = ((UInt64)sfmt[a + 1] << 32) | sfmt[a + 0];
				yh = xh << (SL2_x8) | xl >> (SL2_ix8);
				yl = xl << (SL2_x8);
				xh = ((UInt64)sfmt[c + 3] << 32) | sfmt[c + 2];
				xl = ((UInt64)sfmt[c + 1] << 32) | sfmt[c + 0];
				yh ^= xh >> (SR2_x8);
				yl ^= xl >> (SR2_x8) | xh << (SR2_ix8);

				sfmt[a + 3] = sfmt[a + 3] ^ ((sfmt[b + 3] >> SR1) & MSK4) ^ (sfmt[d + 3] << SL1) ^ ((UInt32)(yh >> 32));
				sfmt[a + 2] = sfmt[a + 2] ^ ((sfmt[b + 2] >> SR1) & MSK3) ^ (sfmt[d + 2] << SL1) ^ ((UInt32)yh);
				sfmt[a + 1] = sfmt[a + 1] ^ ((sfmt[b + 1] >> SR1) & MSK2) ^ (sfmt[d + 1] << SL1) ^ ((UInt32)(yl >> 32));
				sfmt[a + 0] = sfmt[a + 0] ^ ((sfmt[b + 0] >> SR1) & MSK1) ^ (sfmt[d + 0] << SL1) ^ ((UInt32)yl);

				c = d; d = a; a += 4; b += 4;
				if (b >= N32) b = 0;
			} while (a < N32);
#endif

		}

        /// <summary>
        /// gen_rand_allの(2^19937-1)周期用。
        /// </summary>
		private void gen_rand_all_19937()
		{
            int a, b, c, d;
            UInt32[] p = this.sfmt;

            const int cMEXP = 19937;
            const int cPOS1 = 122;
            const uint cMSK1 = 0xdfffffefU;
            const uint cMSK2 = 0xddfecb7fU;
            const uint cMSK3 = 0xbffaffffU;
            const uint cMSK4 = 0xbffffff6U;
            const int cSL1 = 18;
            const int cSR1 = 11;
            const int cN = cMEXP / 128 + 1;
            const int cN32 = cN * 4;

            a = 0;
            b = cPOS1 * 4;
            c = (cN - 2) * 4;
            d = (cN - 1) * 4;
            do {
                p[a + 3] = p[a + 3] ^ (p[a + 3] << 8) ^ (p[a + 2] >> 24) ^ (p[c + 3] >> 8) ^ ((p[b + 3] >> cSR1) & cMSK4) ^ (p[d + 3] << cSL1);
                p[a + 2] = p[a + 2] ^ (p[a + 2] << 8) ^ (p[a + 1] >> 24) ^ (p[c + 3] << 24) ^ (p[c + 2] >> 8) ^ ((p[b + 2] >> cSR1) & cMSK3) ^ (p[d + 2] << cSL1);
                p[a + 1] = p[a + 1] ^ (p[a + 1] << 8) ^ (p[a + 0] >> 24) ^ (p[c + 2] << 24) ^ (p[c + 1] >> 8) ^ ((p[b + 1] >> cSR1) & cMSK2) ^ (p[d + 1] << cSL1);
                p[a + 0] = p[a + 0] ^ (p[a + 0] << 8) ^ (p[c + 1] << 24) ^ (p[c + 0] >> 8) ^ ((p[b + 0] >> cSR1) & cMSK1) ^ (p[d + 0] << cSL1);
                c = d; d = a; a += 4; b += 4;
                if (b >= cN32) b = 0;
            } while (a < cN32);
        }
#endregion
    }

}