import requests  

class BotHandler:

    def __init__(self, token):
        self.token = token
        self.api_url = "https://api.telegram.org/bot{}/".format(token)
        self.offset = None
        self.timeout = 0

    def get_updates(self):
        method = 'getUpdates'
        params = {'timeout': self.timeout, 'offset': self.offset}
        resp = requests.get(self.api_url + method, params)
        result_json = resp.json()['result']
        if len(result_json) > 0:
            self.offset = result_json[0]['update_id'] + 1
        else:
            self.offset = None
        return result_json

    def send_message(self, chat_id, text):
        params = {'chat_id': chat_id, 'text': text}
        method = 'sendMessage'
        resp = requests.post(self.api_url + method, params)
        return resp

    def reply_to_message(self, chat_id, text, reply_to_message_id):
        params = {'chat_id': chat_id, 'text': text, 'reply_to_message_id': reply_to_message_id}
        method = 'sendMessage'
        resp = requests.post(self.api_url + method, params)
        return resp

mybot = BotHandler('649982709:AAEH6u7pmW6deVuJWfYDNOj5I5u-dXaN2rI');

class TranslaThor:

    def __init__(self, api_key):
        self.api_key = api_key
        self.api_url = 'https://translate.yandex.net/api/v1.5/tr.json/'

    def detect(self, text_for_detection):
        method = 'detect'
        params = {'key': self.api_key, 'text': text_for_detection}
        resp = requests.get(self.api_url + method, params) #prox
        detected = resp.json()
        return detected

    def translate(self, text_for_translation, lang = 'en'):
        method = 'translate'
        params = {'key': self.api_key, 'text': text_for_translation, 'lang': lang}
        resp = requests.get(self.api_url + method, params) #prox
        translated = resp.json()['text']
        return translated
    
translator = TranslaThor('trnsl.1.1.20181209T145918Z.7dd52327550b6c7e.ed590e7965dad56809bb01fe77f376e97aa7619b')

def main():
    update = mybot.get_updates()
    if len(update) == 0:
        mybot.timeout = 100
    else:
        mybot.timeout = 100
        mybot.offset = update[-1]['update_id'] + 1
    while 1:
        update = mybot.get_updates()
        if len(update) > 0:
            if('message' in update[0]):
                if ('text' in update[0]['message']):
                    text_for_translation = update[0]['message']['text']
                    chat_id = update[0]['message']['chat']['id']
                    reply_to_message_id = update[0]['message']['message_id']
                    lang = translator.detect(text_for_translation)['lang']
                    if (lang != 'en' and lang != ''):
                        translated_text = translator.translate(text_for_translation)
                        mybot.reply_to_message(chat_id, translated_text, reply_to_message_id)

if __name__ == '__main__':  
    try:
        main()
    except KeyboardInterrupt:
        exit()
