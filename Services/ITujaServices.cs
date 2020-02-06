using System.Threading.Tasks;

namespace TujaEsploristoNet.Services
{
	public interface ITujaServices
	{
		bool Krei(string path, string kio);
		Task <bool> KreiAsync(string path, string v);

		int statuto(int stat);
	}
}