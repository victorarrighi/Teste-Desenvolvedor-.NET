using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;

namespace VestibularAPI.Models
{
    public class ProcessoSeletivo //entidade que representa um processo seletivo
    {
        [Key] //definindo id como chave primária
        public int Id { get; set; }
        public string Nome { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataTermino { get; set; }
    }

    
    public class Lead
    {
        [Key]
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public string CPF { get; set; }
    }

    //entidade que representa uma oferta de curso
    public class Oferta
    {
        [Key]
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public int VagasDisponiveis { get; set; }
    }

    //entidade que representa uma inscrição
    public class Inscricao
    {
        [Key]
        public int Id { get; set; }
        public int NumeroInscricao { get; set; }
        public DateTime Data { get; set; }
        public string Status { get; set; }

    //foreignkey de onde Inscrição vai se relacionar
        [ForeignKey("Lead")]
        public int LeadId { get; set; }
        public Lead Lead { get; set; }

        [ForeignKey("ProcessoSeletivo")]
        public int ProcessoSeletivoId { get; set; }
        public ProcessoSeletivo ProcessoSeletivo { get; set; }

        [ForeignKey("Oferta")]
        public int OfertaId { get; set; }
        public Oferta Oferta { get; set; }
    }

    // para gerenciar as entidades e suas relações com o banco de dados
    public class VestibularContext : DbContext
    {
        //definindo o nome da connection string
        public VestibularContext() : base("VestibularDB") { }

        //conjuntos de entidades que representam tabelas no banco de dados
        public DbSet<ProcessoSeletivo> ProcessosSeletivos { get; set; }
        public DbSet<Lead> Leads { get; set; }
        public DbSet<Oferta> Ofertas { get; set; }
        public DbSet<Inscricao> Inscricoes { get; set; }
    }

    // falar depois
    public class InscricaoRepository
    {
        private readonly VestibularContext _context;

        public InscricaoRepository()
        {
            _context = new VestibularContext();
        }

        
        public List<Inscricao> GetAll()//retorna todas as inscrições
        {
            return _context.Inscricoes.Include("Lead").Include("Oferta").Include("ProcessoSeletivo").ToList();
        }

        
        public Inscricao GetById(int id)//busca uma inscrição pelo ID - vai ser chamado no getbyid
        {
            return _context.Inscricoes.Include("Lead").Include("Oferta").Include("ProcessoSeletivo").FirstOrDefault(i => i.Id == id);
        }

        public void Add(Inscricao inscricao) //adiciona uma nova inscrição - vai ser chamado no post
        {
            _context.Inscricoes.Add(inscricao);
            _context.SaveChanges();
        }

        public void Update(Inscricao inscricao) //atualiza uma inscrição - vai ser chamado no put
        {
            var existing = _context.Inscricoes.Find(inscricao.Id);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(inscricao);
                _context.SaveChanges();
            }
        }

        public void Delete(int id) //remove uma inscrição pelo ID - vai ser chamado no delete
        {
            var inscricao = _context.Inscricoes.Find(id);
            if (inscricao != null)
            {
                _context.Inscricoes.Remove(inscricao);
                _context.SaveChanges();
            }
        }

        public List<Inscricao> GetByCpf(string cpf) //método que recebe cpf e retorna todas inscrições vinculadas
        {
            return _context.Inscricoes
                .Include("Lead").Include("Oferta").Include("ProcessoSeletivo")
                .Where(i => i.Lead.CPF == cpf).ToList();
        }

        public List<Inscricao> GetByOferta(string oferta) //método que recebe a oferta e retorna todas inscrições associadas
        {
            return _context.Inscricoes
                .Include("Lead").Include("Oferta").Include("ProcessoSeletivo")
                .Where(i => i.Lead.Oferta == oferta).ToList();
        }
    }

    [RoutePrefix("api/inscricoes")] //controller para mostrar os endpoints da api
    public class InscricaoController : ApiController
    {
        private readonly InscricaoRepository _repository = new InscricaoRepository(); //criação do objeto para acessar os métodos que lidam com o banco

        // GET api/inscricoes
        [HttpGet, Route("")]
        public IHttpActionResult GetAll() //vai obter todos as inscrições cadastradas no banco 
        {
            return Ok(_repository.GetAll()); //usei return ok() para retornar as inscrições com status 200
        }

        // GET api/inscricoes/{id}
        [HttpGet, Route("{id}")]
        public IHttpActionResult GetById(int id) //faz o get via id
        {
            var inscricao = _repository.GetById(id);
            if (inscricao == null)
                return NotFound(); //retorna 404
            return Ok(inscricao); // retorna 200
        }

        // POST api/inscricoes
        [HttpPost, Route("")]
        public IHttpActionResult Add([FromBody] Inscricao inscricao) //o frombody indicando que os dados vao vir no corpo (json)
        {
            if (inscricao == null)
                return BadRequest("Dados inválidos.");
            _repository.Add(inscricao); //chama o metodo Add levando a inscrição como parametro
            return Ok(inscricao);
        }

        // PUT api/inscricoes/{id}
        [HttpPut, Route("{id}")]
        public IHttpActionResult Update(int id, [FromBody] Inscricao inscricao)
        {
            if (inscricao == null || inscricao.Id != id)
                return BadRequest("Dados inválidos.");
            _repository.Update(inscricao);
            return Ok(inscricao);
        }

        // DELETE api/inscricoes/{id}
        [HttpDelete, Route("{id}")]
        public IHttpActionResult Delete(int id)
        {
            _repository.Delete(id);
            return Ok();
        }

        // GET api/inscricoes/cpf/{cpf} => busca todas as inscrições por CPF
        [HttpGet, Route("cpf/{cpf}")]
        public IHttpActionResult GetByCpf(string cpf)
        {
            var inscricoes = _repository.GetByCpf(cpf);
            return Ok(inscricoes);
        }

        // GET api/inscricoes/oferta/{oferta} => busca todas as inscrições por oferta
        [HttpGet, Route("oferta/{oferta}")]
        public IHttpActionResult GetByOferta(string oferta)
        {
            var inscricoes = _repository.GetByOferta(oferta);
            return Ok(inscricoes);
        }
    }
}
