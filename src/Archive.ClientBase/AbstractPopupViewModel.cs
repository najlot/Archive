using System;

namespace Archive.ClientBase
{
	public interface IPopupViewModel { }

	public abstract class AbstractPopupViewModel<T> : AbstractViewModel, IPopupViewModel
	{
		public Action<T> SetResult { get; set; }
	}
}
