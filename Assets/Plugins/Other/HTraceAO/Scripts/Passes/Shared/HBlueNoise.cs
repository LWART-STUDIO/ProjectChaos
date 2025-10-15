using UnityEngine;
using UnityEngine.Rendering;

namespace HTraceAO.Scripts.Passes.Shared
{
	internal static class HBlueNoise
	{
		private static readonly  int g_OwenScrambledTexture = Shader.PropertyToID("g_OwenScrambledTexture");
		private static readonly int g_ScramblingTileXSPP   = Shader.PropertyToID("g_ScramblingTileXSPP");
		private static readonly int g_RankingTileXSPP      = Shader.PropertyToID("g_RankingTileXSPP");
		private static readonly int g_ScramblingTexture    = Shader.PropertyToID("g_ScramblingTexture");
		
		private static         Texture2D _owenScrambledTexture;
		public static Texture2D OwenScrambledTexture
		{
			get
			{
				if (_owenScrambledTexture == null)
					_owenScrambledTexture = UnityEngine.Resources.Load<Texture2D>("HTraceAO/BlueNoise/OwenScrambledNoise256");
				return _owenScrambledTexture;
			}
		}
		
		private static Texture2D _scramblingTileXSPP;
		public static Texture2D ScramblingTileXSPP
		{
			get
			{
				if (_scramblingTileXSPP == null)
					_scramblingTileXSPP = UnityEngine.Resources.Load<Texture2D>("HTraceAO/BlueNoise/ScramblingTile8SPP");
				return _scramblingTileXSPP;
			}
		}
		private static Texture2D _rankingTileXSPP;
		public static Texture2D RankingTileXSPP
		{
			get
			{
				if (_rankingTileXSPP == null)
					_rankingTileXSPP = UnityEngine.Resources.Load<Texture2D>("HTraceAO/BlueNoise/RankingTile8SPP");
				return _rankingTileXSPP;
			}
		}
		private static Texture2D _scramblingTexture;
		public static Texture2D ScramblingTexture
		{
			get
			{
				if (_scramblingTexture == null)
					_scramblingTexture = UnityEngine.Resources.Load<Texture2D>("HTraceAO/BlueNoise/ScrambleNoise");
				return _scramblingTexture;
			}
		}
		
		public static void SetTextures(CommandBuffer cmd)
		{
			cmd.SetGlobalTexture(g_OwenScrambledTexture, OwenScrambledTexture);
			cmd.SetGlobalTexture(g_ScramblingTileXSPP,   ScramblingTileXSPP);
			cmd.SetGlobalTexture(g_RankingTileXSPP,      RankingTileXSPP);
			cmd.SetGlobalTexture(g_ScramblingTexture,    ScramblingTexture);
		}
	}
}
