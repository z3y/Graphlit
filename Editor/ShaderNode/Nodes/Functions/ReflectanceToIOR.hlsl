void ReflectanceToIORNode(half Reflectance, out half IOR)
{
	IOR = (1.0 + 0.4 * Reflectance) / (1.0 - 0.4 * Reflectance);
}
