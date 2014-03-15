using System.IO;
using MathNet.Numerics.LinearAlgebra.Double;

namespace AmpIdent.Auxiliaries
{
    class WavLoader
    {
        private readonly DenseMatrix _leftChannel;
        private readonly DenseMatrix _rightChannel;
        private readonly int _loadingPercentage;
        private readonly int _sampleLength;
        private readonly byte[] _wav;

        public DenseMatrix LeftChannel
        {
            get { return _leftChannel; }
        }

        public DenseMatrix RightChannel
        {
            get { return _rightChannel; }
        }

        public int LoadingPercentage
        {
            get { return _loadingPercentage; }
        }

        public int SampleLength
        {
            get { return _sampleLength; }
        }

        public byte[] Wav
        {
            get { return _wav; }
        }

        public WavLoader(string path, int sampleLength, int density)
        {
            // first we need to read our wav file, so we can get our info:
            _wav = File.ReadAllBytes(path);

            // then we are going to get our file's info
            var numChannels = _wav[22];
            //optional
            //var sampleRate = _wav2[24] + 256 * _wav2[25];

            // nr of samples is the length - the 44 bytes that where needed for the offset
            _sampleLength = (_wav.Length - 44) / 2;

            // if there are 2 channels, we need to devide the nr of sample in 2
            if (numChannels == 2) _sampleLength /= 2;
            _sampleLength = _sampleLength / density;

            if (sampleLength != 0)
            {
                _sampleLength = sampleLength;
            }

            var pos = 44; // start of data chunk
            _loadingPercentage = 0;

            // ReSharper disable once CSharpWarnings::CS0618
            _leftChannel = new DenseMatrix(_sampleLength, 1, 0.0);
            // ReSharper disable once CSharpWarnings::CS0618
            _rightChannel = new DenseMatrix(_sampleLength, 1, 0.0);

            var iterationLength = _sampleLength;

            for (var i = 0; i < iterationLength - 1; i++)
            {
                var number = _wav[pos] + 256 * _wav[pos + 1];
                if (number > 32767) number -= 65534;
                _leftChannel[i, 0] = number;

                pos += 2 * numChannels * density;

                _loadingPercentage = i * 50 / iterationLength;
            }
            if (numChannels == 2)
            {
                pos = 44;
                _loadingPercentage = 50;
                for (var i = 0; i < iterationLength - 3; i++)
                {
                    var number = _wav[pos + 2] + 256 * _wav[pos + 3];
                    if (number > 32767) number -= 65534;
                    _rightChannel[i, 0] = number;

                    pos += 2 * numChannels * density;

                    _loadingPercentage = 50 + (i * 50 / iterationLength);
                }
            }

            _loadingPercentage = 100;
        }
    }
}
