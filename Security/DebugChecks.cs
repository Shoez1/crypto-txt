using System;
using System.Collections.Generic;

namespace CryptoTxt.Security
{
    /// <summary>
    /// Separa as checagens nativas (rápidas, seguras para repetir em um watchdog)
    /// das heurísticas de ambiente (processos/janelas), caras e propensas a falsos positivos.
    /// </summary>
    internal sealed class DebugChecks
    {
        private readonly IReadOnlyList<Func<bool>> _nativeProbes;
        private readonly IReadOnlyList<Func<bool>> _heuristicProbes;

        internal DebugChecks(IReadOnlyList<Func<bool>> nativeProbes, IReadOnlyList<Func<bool>> heuristicProbes)
        {
            _nativeProbes = nativeProbes ?? throw new ArgumentNullException(nameof(nativeProbes));
            _heuristicProbes = heuristicProbes ?? throw new ArgumentNullException(nameof(heuristicProbes));
        }

        internal bool IsFast() => Evaluate(_nativeProbes);

        internal bool IsFull() => Evaluate(_nativeProbes) || Evaluate(_heuristicProbes);

        private static bool Evaluate(IReadOnlyList<Func<bool>> probes)
        {
            foreach (Func<bool> probe in probes)
            {
                try
                {
                    if (probe())
                    {
                        return true;
                    }
                }
                catch
                {
                    // Uma checagem indisponível não deve desativar as demais.
                }
            }

            return false;
        }
    }
}
