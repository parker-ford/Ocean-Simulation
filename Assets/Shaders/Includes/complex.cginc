struct Complex {
    float real;
    float im;
};

Complex mul(Complex c0, Complex c1){
    Complex c;
    c.real = c0.real * c1.real - c0.im * c1.im;
    c.im = c0.real * c1.im + c0.im * c1.real;
    return c;
}

Complex add(Complex c0, Complex c1) {
    Complex c;
    c.real = c0.real + c1.real;
    c.im = c0.im + c1.im;
    return c;
}

Complex conj(Complex c){
    Complex c_conj;
    c_conj.real = c.real;
    c_conj.im = -c.im;
    return c_conj;
}

float2 c_exp(float2 c){
    return float2(cos(c.y), sin(c.y)) * exp(c.x);
}

float2 ComplexMult(float2 a, float2 b)
{
	return float2(a.r * b.r - a.g * b.g, a.r * b.g + a.g * b.r);
}