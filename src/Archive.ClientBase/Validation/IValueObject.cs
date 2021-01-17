using System.Collections.Generic;

namespace Archive.ClientBase.Validation
{
	public interface IValueObject { IEnumerable<ValidationResult> Validate(); }
}
