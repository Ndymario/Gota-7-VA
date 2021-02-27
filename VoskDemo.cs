using System;
using System.IO;
using Vosk;

public class VoskDemo
{
   public static void DemoBytes(Model model)
   {
        // Demo byte buffer
        VoskRecognizer rec = new VoskRecognizer(model, 16000.0f);
        using(Stream source = File.OpenRead("test.wav")) {
            byte[] buffer = new byte[4096];
            int bytesRead;
            while((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0) {
                if (rec.AcceptWaveform(buffer, bytesRead)) {
                    Console.WriteLine(rec.Result());
                } else {
                    Console.WriteLine(rec.PartialResult());
                }
            }
        }
        Console.WriteLine(rec.FinalResult());
   }

    static void DoThing()
    {
        var recorders = AudioCapture.AvailableDevices;
        for (int i = 0; i < recorders.Count; i++)
        {
            Console.WriteLine(recorders[i]);
        }
        Console.WriteLine("-----");

        const int samplingRate = 44100;     // Samples per second

        const ALFormat alFormat = ALFormat.Mono16;
        const ushort bitsPerSample = 16;    // Mono16 has 16 bits per sample
        const ushort numChannels = 1;       // Mono16 has 1 channel

        using (var f = File.OpenWrite("out.wav"))

        using (var sw = new BinaryWriter(f))
        {
            // Read This: http://soundfile.sapp.org/doc/WaveFormat/

            sw.Write(new char[] { 'R', 'I', 'F', 'F' });
            sw.Write(0); // will fill in later
            sw.Write(new char[] { 'W', 'A', 'V', 'E' });
            // "fmt " chunk (Google: WAVEFORMATEX structure)
            sw.Write(new char[] { 'f', 'm', 't', ' ' });
            sw.Write(16); // chunkSize (in bytes)
            sw.Write((ushort)1); // wFormatTag (PCM = 1)
            sw.Write(numChannels); // wChannels
            sw.Write(samplingRate); // dwSamplesPerSec
            sw.Write(samplingRate * numChannels * (bitsPerSample / 8)); // dwAvgBytesPerSec
            sw.Write((ushort)(numChannels * (bitsPerSample / 8))); // wBlockAlign
            sw.Write(bitsPerSample); // wBitsPerSample
            // "data" chunk
            sw.Write(new char[] { 'd', 'a', 't', 'a' });
            sw.Write(0); // will fill in later

            // 10 seconds of data. overblown, but it gets the job done
            const int bufferLength = samplingRate * 10;
            int samplesWrote = 0;

            Console.WriteLine($"Recording from: {recorders[0]}");

            using (var audioCapture = new AudioCapture(
                recorders[0], samplingRate, alFormat, bufferLength))
            {
                var buffer = new short[bufferLength];

                audioCapture.Start();
                for (int i = 0; i < 10; ++i)
                {
                    Thread.Sleep(1000); // give it some time to collect samples

                    var samplesAvailable = audioCapture.AvailableSamples;
                    audioCapture.ReadSamples(buffer, samplesAvailable);
                    for (var x = 0; x < samplesAvailable; ++x)
                    {
                        sw.Write(buffer[x]);
                    }

                    samplesWrote += samplesAvailable;

                    Console.WriteLine($"Wrote {samplesAvailable}/{samplesWrote} samples...");
                }
                audioCapture.Stop();
            }

            sw.Seek(4, SeekOrigin.Begin); // seek to overall size
            sw.Write(36 + samplesWrote * (bitsPerSample / 8) * numChannels);
            sw.Seek(40, SeekOrigin.Begin); // seek to data size position
            sw.Write(samplesWrote * (bitsPerSample / 8) * numChannels);
        }
    }

    public static void DemoFloats(Model model)
   {
        // Demo float array
        VoskRecognizer rec = new VoskRecognizer(model, 16000.0f);
        using(Stream source = File.OpenRead("test.wav")) {
            byte[] buffer = new byte[4096];
            int bytesRead;
            while((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0) {
                float[] fbuffer = new float[bytesRead / 2];
                for (int i = 0, n = 0; i < fbuffer.Length; i++, n+=2) {
                    fbuffer[i] = (short)(buffer[n] | buffer[n+1] << 8);
                }
                if (rec.AcceptWaveform(fbuffer, fbuffer.Length)) {
                    Console.WriteLine(rec.Result());
                    GC.Collect();
                } else {
                    Console.WriteLine(rec.PartialResult());
                }
            }
        }
        Console.WriteLine(rec.FinalResult());
   }

   public static void DemoSpeaker(Model model)
   {
        // Output speakers
        SpkModel spkModel = new SpkModel("model-spk");
        VoskRecognizer rec = new VoskRecognizer(model, spkModel, 16000.0f);

        using(Stream source = File.OpenRead("test.wav")) {
            byte[] buffer = new byte[4096];
            int bytesRead;
            while((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0) {
                if (rec.AcceptWaveform(buffer, bytesRead)) {
                    Console.WriteLine(rec.Result());
                } else {
                    Console.WriteLine(rec.PartialResult());
                }
            }
        }
        Console.WriteLine(rec.FinalResult());
   }

   public static void Main()
   {
        DoThing();

        // You can set to -1 to disable logging messages
        Vosk.Vosk.SetLogLevel(0);

        Model model = new Model("model");
        DemoBytes(model);
        DemoFloats(model);
        DemoSpeaker(model);
   }
}
