// cs0052.cs: Inconsistent accessibility: field type `InternalClass' is less accessible than field 'PublicClass.member'
// Line: 8

class InternalClass {
}

public class PublicClass {
	public InternalClass member;
}
