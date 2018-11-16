var isTouched = script.addBoolParameter("isTouched","Is Touched",false);

function moduleValueChanged(value)
{
	isTouched.set(value.get() > 0);
}