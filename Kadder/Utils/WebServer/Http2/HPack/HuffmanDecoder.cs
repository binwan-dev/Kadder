using System;
using System.IO;

namespace Kadder.Utils.WebServer.Http2.HPack
{
    public class HuffmanDecoder
    {
        private Node _root = null;

        public HuffmanDecoder(int[] codes, byte[] lengths)
        {
            if (codes.Length != 257 || codes.Length != lengths.Length)
                throw new ArgumentException("Invalid huffman coding!");
            _root = buildTree(codes, lengths);
        }

        public byte[] Decode(byte[] buffer)
        {
            using (var result = new MemoryStream())
            {
                var node = _root;
                var current = 0;
                var bits = 0;
                for (var i = 0; i < buffer.Length; i++)
                {
                    var b = buffer[i] & 255;
                    current = (current << 8) | b;
                    bits += 8;
                    while (bits >= 8)
                    {
                        var c = (current >> (bits - 8)) & 255;
                        node = node.Childrens[c];
                        bits -= node.Bits;
                        if (node.IsTerminal())
                        {
			    if(node.Symbol==HPackUtil.HUFFMAN_EOS)
                                throw new IOException("EOS Decoder");
                            result.WriteByte((byte)node.Symbol);
                            node = _root;
                        }
                    }
                }

                while (bits > 0)
                {
                    var c = (current << (8 - bits)) & 255;
                    node = node.Childrens[c];
                    if (node.IsTerminal() && node.Bits <= bits)
                    {
                        bits -= node.Bits;
                        result.WriteByte((byte)node.Symbol);
                        node = _root;
                    }
		    else
		    {
                        break;
                    }
                }
		
		// Section 5.2. String Literal Representation
		// Padding not corresponding to the most significant bits of the code
		// for the EOS symbol (0xFF) MUST be treated as a decoding error.
		var mask = (1 << bits) - 1;
                if ((current & mask) != mask)
                    throw new IOException("Invalid Padding");

                return result.ToArray();
            }
        }

        public class Node
        {
            private int _symbol;

            private int _bits;

            private Node[] _childrens;

            public int Symbol { get; }

            public int Bits { get; }

            public Node[] Childrens { get; }

            public Node() : this(0, 0, new Node[256])
            {
            }

            public Node(int symbol, int bits, Node[] childrens = null)
            {
                Symbol = symbol;
                Bits = bits;
                Childrens = childrens;
            }

            public bool IsTerminal() => Childrens.Length == 0 ? true : false;
        }

        private static Node buildTree(int[] codes, byte[] lengths)
        {
            var root = new Node();
            for (var i = 0; i < codes.Length; i++)
                insert(root, i, codes[i], lengths[i]);
            return root;
        }

        private static void insert(Node root, int symbol, int code, byte length)
        {
            var current = root;
            while (length > 8)
            {
                if (current.IsTerminal())
                    throw new InvalidDataException("Invalid huffman code: prefix not unique!");

                length -= 8;
                var i = (code >> length) & 255;
                if (current.Childrens[i] == null)
                    current.Childrens[i] = new Node();
                current = current.Childrens[i];
            }

            var terminal = new Node(symbol, length);
            var shift = 8 - length;
            var start = (code << shift) & 255;
            var end = 1 << shift;
            for (var i = start; i < start + end; i++)
                current.Childrens[i] = terminal;
        }
    }
}
