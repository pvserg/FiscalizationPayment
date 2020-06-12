using System;
using System.Collections.Concurrent;
using System.Threading;
using NLog;

namespace AAP.OTAFinance.Integration.Uniteller.FiscalizationPayment
{
    /// <summary>
    /// Класс, реализующий простейший шаблон Producer - Consumer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AsyncPCQueue<T> : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetLogger("AsyncPCQueue");

        private readonly Action<T> _Consumer;
        private readonly BlockingCollection<T> _Queue;
        private readonly CancellationTokenSource _CancelTokenSrc;

        public AsyncPCQueue(Action<T> consumer)
        {
            if (consumer == null)
            {
                throw new ArgumentNullException(nameof(consumer));
            }
            _Consumer = consumer;
            _Queue = new BlockingCollection<T>(new ConcurrentQueue<T>());
            _CancelTokenSrc = new CancellationTokenSource();

            new Thread(() => ConsumeLoop(_CancelTokenSrc.Token)).Start();
        }

        public void Add(T value)
        {
            _Queue.Add(value);
            //TODO: временный контроль кол-ва элементов
            //Logger.Debug("Добавлен новый элемент в очередь, общее кол-во элементов в очереди {0}", _Queue.Count);
            if (_Queue.Count > 1000 && _Queue.Count < 100)
            {
                Logger.Warn("В очереди больше 1000 элементов типа {0}", typeof(T).FullName);
            }
            else if (_Queue.Count > 100)
            {
                Logger.Warn("В очереди больше 100 элементов типа {0}", typeof(T).FullName);
            }
        }

        private void ConsumeLoop(CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                try
                {
                    var item = _Queue.Take(cancelToken);
                    _Consumer(item);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "AsyncPCQueue  ErrorException: ");
                }
            }
        }

        #region IDisposable

        private bool m_isDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!m_isDisposed)
            {
                if (disposing)
                {
                    _CancelTokenSrc.Cancel();
                    _CancelTokenSrc.Dispose();
                    _Queue.Dispose();
                }

                m_isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
