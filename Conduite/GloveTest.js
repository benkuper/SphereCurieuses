var isTouched = script.addBoolParameter("isTouched","Is Touched",false);

function moduleValueParamChanged(value)
{
	isTouched.set(value.get() > 0);
}