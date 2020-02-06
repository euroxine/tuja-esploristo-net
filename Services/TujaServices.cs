using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TujaEsploristo;

namespace TujaEsploristoNet.Services
{
	public class TujaServices : ITujaServices
	{
		private readonly ILogger<TujaServices> logger;
        private int _statuto = 0;
		public TujaServices(ILogger<TujaServices> logger)
		{
			this.logger = logger;
		}
        public int statuto(int kio)
        {
            if (kio != -1)
            {
                _statuto = kio;
            }
            return _statuto;
        }
        public bool Krei(string path, string kio)
		{
			TujaDosierujo tujaDosierujo = new TujaDosierujo(path);
            switch (kio.ToLower())
            {
                case "espdic":
                    tujaDosierujo.KreiESPDIC();
                    break;
                case "etimologio":
                    tujaDosierujo.KreiEtimologioj();
                    break;
                case "utf8xml":
                    tujaDosierujo.Unikodiĝi();
                    break;
                case "vortaroj":
                    tujaDosierujo.KreiVortarojn();
                    break;
                case "difinoj":
                    tujaDosierujo.KreiDifinojn();
                    break;
                case "eo_fr":
                    tujaDosierujo.ŝlosiliguPorPlenumi();
                    break;
                case "eo_fr_eo":
                    tujaDosierujo.DeŝlosiliguPorPlenumi();
                    break;
                case "legutest":
                    tujaDosierujo.LeguTest();
                    break;
            }
            logger.LogInformation($"path: {path}");
            return true;
		}
        public Task<bool> KreiAsync(string path, string kio)
        {
            if (kio == "legu")
            {
                TujaDosierujo tujaDosierujo = new TujaDosierujo(path);
                return tujaDosierujo.Legu();
            }
            return Task<bool>.Run( () => Krei(path,kio));
        }
    }
}
