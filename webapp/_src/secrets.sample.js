(() => {
    console.info('bootstrapping element');

    const quadWord = '';
    const incident = 1921;
    const env = 'Fire';

    const appNode = document.getElementById('incident-form:j_idt102');
    appNode.setAttribute('data-dojo-props', `incidentId: ${incident}, quadWord: '${quadWord}', env: '${env}'`);
})();
