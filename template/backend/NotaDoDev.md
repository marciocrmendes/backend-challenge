Olá, tudo bem?

Vou explicar o porque de algumas decisões de design e implementação que tomei durante o desenvolvimento do projeto.

Exceptions:
	Eu particularmente prefiro tratar as exceptions de forma global e sendo somente exceção de fato, ou seja, alguma situação não tratada
	optando por utilizar Result Pattern ou Notification Pattern para retorno de mensagens de erro de domínio.
	Mas para esse teste achei melhor respeitar a proposta de vocês e utilizei as exceptions para retornar mensagens de erro

Arquivo: SalesController
Linha: 47
Motivo: 
	Decidi seguir assim pra facilitar os testes. Entendi previamente que essa api era uma de varias referentes ao contexto de vendas,
	e que no ambiente produtivo não teríamos a necessidade de cadastrar usuario com role na base de dados, mas achei que ficaria 
	mais interessante assim também pela possibilidade de trabalhar com extração de claims para autorização, já que o token de autenticação
	viria pela api de autenticação e seria propagada para as outras apis.
	Resumindo, criei apenas para deixar o teste mais interessante nesse sentido.