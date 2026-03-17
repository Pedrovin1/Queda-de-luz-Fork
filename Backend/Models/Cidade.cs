public class Cidade
{
    public int Id {get; private set;} = -1 ;
    public string Nome {get; private set;}
    public string EstadoSigla {get; private set;}

    public Cidade(){}

    public Cidade(long Id, string Nome, string EstadoSigla)
    {
        this.Id = (int)Id;
        this.Nome = Nome;
        this.EstadoSigla = EstadoSigla;
    }

    public Cidade(string nome, string estadoSigla)
    {
        this.Nome = nome;
        this.EstadoSigla = estadoSigla;
    }
}