using System;
using UnityEngine;

namespace GarageGauntlet
{
    internal enum RaceSound { Tick, Start, Charge, Boost, Lap, Hit, Finish }

    internal sealed class RaceAudio
    {
        private const string SoundPreference = "garage-gauntlet-sound";
        private const int SampleRate = 22050;
        private readonly AudioSource engine, effects;
        private readonly AudioClip[] cues;
        public bool Enabled { get; private set; }

        public RaceAudio(GameObject owner)
        {
            Enabled = PlayerPrefs.GetInt(SoundPreference, 1) != 0;
            engine = owner.AddComponent<AudioSource>();
            engine.playOnAwake = false;
            engine.loop = true;
            engine.volume = 0;
            engine.clip = MakeEngine();
            effects = owner.AddComponent<AudioSource>();
            effects.playOnAwake = false;
            cues = new[]
            {
                Tone("Countdown", .12f, 490, 440),
                Tone("Start", .32f, 580, 900),
                Tone("Battery", .26f, 570, 1170),
                Tone("Boost", .32f, 250, 580),
                Tone("Lap", .30f, 670, 890),
                Tone("Impact", .31f, 112, 47, .54f),
                Tone("Finish", .68f, 490, 985)
            };
        }

        public void Begin()
        {
            if (Enabled && !engine.isPlaying)
                engine.Play();
        }

        public void Toggle()
        {
            Enabled = !Enabled;
            PlayerPrefs.SetInt(SoundPreference, Enabled ? 1 : 0);
            PlayerPrefs.Save();
            if (Enabled)
                Begin();
            else
            {
                engine.Stop();
                effects.Stop();
            }
        }

        public void Signal(RaceSound sound)
        {
            if (Enabled)
                effects.PlayOneShot(cues[(int)sound], .44f);
        }

        public void Update(float speed, bool boosting, bool active)
        {
            float motor = Mathf.Clamp01(Mathf.Abs(speed) / 15);
            float level = Enabled && active ? .085f + motor * .13f : 0;
            engine.volume = Mathf.MoveTowards(engine.volume, level, Time.unscaledDeltaTime * .8f);
            engine.pitch = .69f + motor * .89f + (boosting ? .2f : 0);
        }

        private static AudioClip MakeEngine()
        {
            var samples = new float[SampleRate / 2];
            for (int i = 0; i < samples.Length; i++)
            {
                double time = i / (double)SampleRate;
                double phase = time * Math.PI * 2;
                samples[i] = (float)(Math.Sin(phase * 112) * .32 + Math.Sin(phase * 224) * .12 +
                    Math.Sin(phase * 344) * .07 + Math.Sin(phase * 470) * .055);
            }
            return MakeClip("RC motor", samples);
        }

        private static AudioClip Tone(string name, float duration, float start, float end, float noise = 0)
        {
            var samples = new float[Mathf.RoundToInt(duration * SampleRate)];
            var random = new System.Random(733);
            float phase = 0;
            for (int i = 0; i < samples.Length; i++)
            {
                float progress = i / (float)samples.Length;
                phase += Mathf.Lerp(start, end, progress) * Mathf.PI * 2 / SampleRate;
                float amplitude = Mathf.Min(1, progress * 45) * Mathf.Pow(1 - progress, 1.7f);
                samples[i] = (Mathf.Sin(phase) * .63f + Mathf.Sin(phase * 2) * .12f +
                    ((float)random.NextDouble() * 2 - 1) * noise) * amplitude;
            }
            return MakeClip(name, samples);
        }

        private static AudioClip MakeClip(string name, float[] samples)
        {
            AudioClip clip = AudioClip.Create(name, samples.Length, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
