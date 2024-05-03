namespace Kadder.Utils.WebServer.Http2.HPack
{
    public class Huffman
    {
        public static HuffmanDecoder Decoder = new HuffmanDecoder(HPackUtil.HUFFMAN_CODES, HPackUtil.HUFFMAN_CODE_LENGTHS);
    }
}
