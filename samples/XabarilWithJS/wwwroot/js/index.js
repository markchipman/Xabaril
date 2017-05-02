var app = new Vue({
    el: '#app',
    data: {        
        xabarilUrl: `${window.location.href}/xabaril?featureName=`,
        features: ['UtcFeature#1', 'FromToFeature#1', 'UserAuthenticatedFeature#1'],
        featuresStatus : []
    },   
    methods: {         
      
        testFeatures() {
            this.clear();

            this.features.forEach(feature => {
                let featurePromise = this.getFeature(feature);
                featurePromise.then(status => {
                    console.log(status);
                    this.featuresStatus.push({ name: feature, status });
                });
            });
        },
        clear() {
            this.featuresStatus = [];
        },
        getFeature(feature){
            return new Promise((res, rej) => {
                let encodedFeature = encodeURIComponent(feature);
                fetch(`${this.xabarilUrl}${encodedFeature}`)
                     .then( data => data.json().then(res))
                     .catch(rej);
            });
        }
    }
});









