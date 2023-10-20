namespace EZAppMaker.Interfaces
{
	public interface IEZFocusable
	{
		VisualElement FocusedElement { get; }

		void Focus();
		void Unfocus();
	}
}