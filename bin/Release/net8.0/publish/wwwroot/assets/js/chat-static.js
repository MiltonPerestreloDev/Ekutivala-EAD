document.addEventListener("DOMContentLoaded", function () {
    const chatIcon = document.getElementById("chatIcon");
    const chatBox = document.getElementById("chatBox");
    const messagesDiv = document.getElementById("messages");
    const userInput = document.getElementById("userInput");
    const sendButton = document.getElementById("sendButton");
    const closeChat = document.getElementById("closeChat");

    // Perguntas e respostas pré-definidas
    const faq = {
        "1": {
            question: "Sobre Inscrições",
            answer: [
                "<strong>Pergunta: </strong>Como faço para me inscrever nos cursos?<br> <strong> Resposta: </strong> Acesse o site do Ekutivala, escolha o curso desejado e siga as instruções de inscrição. Para cursos pagos, você precisará realizar o pagamento via transferência bancária e enviar o comprovativo via WhatsApp.",
                "<br><br><strong>Pergunta: </strong>Preciso de algum requisito para me inscrever?<br> <strong> Resposta: </strong> Não há requisitos específicos para a maioria dos cursos. Basta ter interesse em aprender o idioma escolhido. Alguns cursos avançados podem exigir conhecimentos prévios, que serão informados na descrição do curso."
            ]
        },
        "2": {
            question: "Sobre Pagamentos",
            answer: [
                "<strong>Pergunta: </strong>Quais são as formas de pagamento aceitas?<br> <strong> Resposta: </strong> Atualmente, aceitamos apenas transferências bancárias. O IBAN será fornecido no momento da inscrição.",
                "<br><br><strong>Pergunta: </strong>Posso cancelar minha inscrição e solicitar reembolso?<br> <strong> Resposta: </strong> Sim, você pode cancelar sua inscrição e solicitar reembolso dentro de 7 dias após a compra, desde que o conteúdo não tenha sido acessado. Após o início do curso, não serão concedidos reembolsos, exceto em casos específicos onde você poderá solicitar o reembolso após 1 ou 2 aulas, recebendo 20% do valor pago."
            ]
        },
        "3": {
            question: "Sobre Acesso aos Cursos",
            answer: [
                "<strong>Pergunta: </strong>Como obtenho acesso aos cursos pagos após o pagamento?<br> <strong> Resposta: </strong> Após enviar o comprovativo de pagamento via WhatsApp, a equipe do Ekutivala confirmará o pagamento e liberará o acesso ao curso em até 48 horas.",
                "<br><br><strong>Pergunta: </strong>Posso acessar os cursos de qualquer dispositivo?<br> <strong> Resposta: </strong> Sim, você pode acessar os cursos de qualquer dispositivo com conexão à internet. Recomendamos o uso de um computador para melhor experiência."
            ]
        },
        "4": {
            question: "Sobre Certificados",
            answer: [
                "<strong>Pergunta: </strong>Recebo um certificado ao concluir um curso?<br> <strong> Resposta: </strong> Sim, ao concluir um curso, você receberá um certificado de participação emitido pelo Ekutivala.",
                "<br><br><strong>Pergunta: </strong>O certificado é reconhecido internacionalmente?<br> <strong> Resposta: </strong> O certificado é emitido pelo Ekutivala e pode ser utilizado para fins acadêmicos ou profissionais. O reconhecimento internacional depende da instituição ou empregador."
            ]
        },
        "5": {
            question: "Sobre Suporte e Contato",
            answer: [
                "<strong>Pergunta: </strong>Como entro em contato com o suporte da Ekutivala?<br> <strong> Resposta: </strong> Você pode entrar em contato conosco através do e-mail ekutivalasuporte@gmail.com ou pelo WhatsApp disponível no site. Nossa equipe está disponível para ajudar com dúvidas e problemas.",
                "<br><br><strong>Pergunta: </strong>O suporte está disponível 24 horas?<br> <strong> Resposta: </strong> O suporte por e-mail está sempre disponível, mas o atendimento pode levar até 24 horas. O atendimento via WhatsApp tem horário comercial."
            ]
        },
        "6": {
            question: "Sobre Políticas de Privacidade",
            answer: [
                "<strong>Pergunta: </strong>Como a Ekutivala protege meus dados pessoais?<br> <strong> Resposta: </strong> A Ekutivala implementa medidas de segurança avançadas, como criptografia e backups regulares, para proteger seus dados. Além disso, seguimos rigorosamente as políticas de privacidade descritas em nosso site.",
                "<br><br><strong>Pergunta: </strong>Posso solicitar a exclusão dos meus dados?<br> <strong> Resposta: </strong> Sim, você pode solicitar a exclusão dos seus dados a qualquer momento, exceto quando a retenção for necessária para cumprir obrigações legais."
            ]
        },
        "7": {
            question: "Outras Dúvidas",
            answer: [
                "<strong>Pergunta: </strong>O que faço se minha dúvida não está listada aqui?<br> <strong> Resposta: </strong> Se sua dúvida não foi respondida, entre em contato com o suporte da Ekutivala através do e-mail ekutivalasuporte@gmail.com ou pelo WhatsApp disponível no site.",
                "<br><br><strong>Pergunta: </strong>Posso sugerir novos cursos ou idiomas?<br> <strong> Resposta: </strong> Sim, adoramos sugestões! Envie sua ideia para sugestoes@ekutivala.org."
            ]
        }
    };




    // Alternar exibição do chat
    function toggleChat() {
        if (chatBox.style.display === "none" || chatBox.style.display === "") {
            chatBox.style.display = "block";
            loadFAQ(); // Carregar perguntas frequentes ao abrir o chat
        } else {
            chatBox.style.display = "none";
        }
    }

    // Fechar chat ao clicar no "X"
    closeChat.addEventListener("click", function () {
        chatBox.style.display = "none";
    });

    // Carregar perguntas frequentes
    function loadFAQ() {
        messagesDiv.innerHTML = "<strong>Escolha uma das opções abaixo:</strong><br>";
        Object.keys(faq).forEach(key => {
            let questionBtn = document.createElement("button");
            questionBtn.innerText = faq[key].question;
            questionBtn.classList.add("faq-button");
            questionBtn.onclick = function () {
                showAnswer(key);
            };
            messagesDiv.appendChild(questionBtn);
            messagesDiv.appendChild(document.createElement("br"));
        });
    }

    // Exibir resposta correspondente
    function showAnswer(key) {
        messagesDiv.innerHTML = `<strong>${faq[key].question}:</strong><br>${faq[key].answer}<br><br>`;
        let backButton = document.createElement("button");
        backButton.innerText = "Voltar";
        backButton.classList.add("back-btn");
        backButton.onclick = loadFAQ;
        messagesDiv.appendChild(backButton);
    }

    // Enviar mensagem do usuário
    function sendMessage() {
        let messageText = userInput.value.trim();
        if (messageText !== "") {
            let newMessage = document.createElement("div");
            newMessage.classList.add("user-message");
            newMessage.innerText = "Você: " + messageText;
            messagesDiv.appendChild(newMessage);
            
            // Responder automaticamente se for uma pergunta conhecida
            let response = Object.values(faq).find(item => item.question.toLowerCase().includes(messageText.toLowerCase()));
            if (response) {
                let botMessage = document.createElement("div");
                botMessage.classList.add("bot-message");
                botMessage.innerText = "Bot: " + response.answer;
                messagesDiv.appendChild(botMessage);
            } else {
                let botMessage = document.createElement("div");
                botMessage.classList.add("bot-message");
                botMessage.innerText = "Bot: Não entendi sua dúvida. Tente novamente ou escolha uma das opções.";
                messagesDiv.appendChild(botMessage);
            }

            userInput.value = ""; // Limpar campo de entrada
            messagesDiv.scrollTop = messagesDiv.scrollHeight; // Rolar para a última mensagem
        }
    }

    // Enviar mensagem ao pressionar Enter
    function checkEnter(event) {
        if (event.key === "Enter") {
            sendMessage();
        }
    }

    // Eventos
    chatIcon.addEventListener("click", toggleChat);
    sendButton.addEventListener("click", sendMessage);
    userInput.addEventListener("keydown", checkEnter);
});
